using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.PowerShellRunner.Abstractions.Accessors;
using Kysect.PowerShellRunner.Abstractions.Queries;
using Kysect.PowerShellRunner.Executions;
using Kysect.PowerShellRunner.Tools;
using Kysect.Zeya.GithubIntegration.Abstraction.Contracts;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using GithubRepository = Kysect.Zeya.Abstractions.Models.GithubRepository;

namespace Kysect.Zeya.GithubIntegration;

public class GithubIntegrationService : IGithubIntegrationService
{
    private readonly IGitHubClient _gitHubClient;
    private readonly IPowerShellAccessor _powerShellAccessor;
    private readonly ILocalStoragePathFactory _pathFormatStrategy;
    private readonly ILogger _logger;

    public GithubIntegrationService(IOptions<GithubIntegrationOptions> githubIntegrationOptions, IGitHubClient gitHubClient, ILocalStoragePathFactory pathFormatStrategy, IPowerShellAccessor powerShellAccessor, ILogger logger)
    {
        githubIntegrationOptions.ThrowIfNull();

        _powerShellAccessor = powerShellAccessor.ThrowIfNull();
        _pathFormatStrategy = pathFormatStrategy.ThrowIfNull();
        _gitHubClient = gitHubClient.ThrowIfNull();
        _logger = logger.ThrowIfNull();
    }

    public void CreatePullRequest(GithubRepository repository, string message)
    {
        repository.ThrowIfNull();
        message.ThrowIfNull();

        string targetPath = _pathFormatStrategy.GetPathToRepository(new GithubUtils.Models.GithubRepository(repository.Owner, repository.Name));
        using (PowerShellPathChangeContext.TemporaryChangeCurrentDirectory(_powerShellAccessor, targetPath))
        {
            _powerShellAccessor.ExecuteAndGet(new PowerShellQuery($"gh pr create --title \"Fix warnings from Zeya\" --body \"{message}\""));
        }
    }
    public bool DeleteBranchOnMerge(GithubRepository githubRepository)
    {
        githubRepository.ThrowIfNull();

        var repositoryInfo = _gitHubClient.Repository.Get(githubRepository.Owner, githubRepository.Name).Result;
        return repositoryInfo.DeleteBranchOnMerge ?? false;
    }

    public RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepository githubRepository, string branch)
    {
        githubRepository.ThrowIfNull();

        try
        {
            BranchProtectionSettings repositoryBranchProtection = _gitHubClient.Repository.Branch.GetBranchProtection(githubRepository.Owner, githubRepository.Name, branch).Result;
            return new RepositoryBranchProtection(repositoryBranchProtection.RequiredPullRequestReviews is not null, repositoryBranchProtection.RequiredConversationResolution?.Enabled ?? false);
        }
        catch (Exception e)
        {
            // TODO: rework this. Possible exception: NotFound, Forbidden (for private repo)
            _logger.LogWarning("Failed to get branch protection info for {Repository}: {Message}", githubRepository.FullName, e.Message);
            return new RepositoryBranchProtection(false, false);
        }
    }
}