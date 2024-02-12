using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.OrganizationsSync.RepositoryDiscovering;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.PowerShellRunner.Abstractions.Accessors;
using Kysect.PowerShellRunner.Abstractions.Queries;
using Kysect.PowerShellRunner.Executions;
using Kysect.PowerShellRunner.Tools;
using Kysect.Zeya.GithubIntegration.Abstraction;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Octokit;
using Repository = LibGit2Sharp.Repository;

namespace Kysect.Zeya.GithubIntegration;

public class GithubIntegrationService : IGithubIntegrationService
{
    private readonly GithubIntegrationCredential _credential;
    private readonly IGitHubClient _gitHubClient;
    private readonly IPowerShellAccessor _powerShellAccessor;
    private readonly ILocalStoragePathFactory _pathFormatStrategy;
    private readonly ILogger _logger;

    public GithubIntegrationService(GithubIntegrationCredential credential, IGitHubClient gitHubClient, ILocalStoragePathFactory pathFormatStrategy, IPowerShellAccessor powerShellAccessor, ILogger logger)
    {
        _credential = credential.ThrowIfNull();

        _powerShellAccessor = powerShellAccessor.ThrowIfNull();
        _pathFormatStrategy = pathFormatStrategy.ThrowIfNull();
        _gitHubClient = gitHubClient.ThrowIfNull();
        _logger = logger.ThrowIfNull();
    }

    public IReadOnlyCollection<GithubRepositoryName> GetOrganizationRepositories(string organization)
    {
        var gitHubRepositoryDiscoveryService = new GitHubRepositoryDiscoveryService(_gitHubClient);
        IReadOnlyList<GithubRepositoryBranch> githubRepositoryBranches = gitHubRepositoryDiscoveryService
            .GetRepositories(organization)
            .Result;

        // TODO: remove this hack after fix https://github.com/kysect/GithubUtils/issues/41
        return githubRepositoryBranches
            .Select(r => new GithubRepositoryName(organization, r.Name))
            .ToList();
    }

    public void CloneOrUpdate(GithubRepositoryName repositoryName)
    {
        repositoryName.ThrowIfNull();

        var repositoryFetchOptions = new RepositoryFetchOptions(_credential.GithubUsername, _credential.GithubToken);
        var repositoryFetcher = new RepositoryFetcher(repositoryFetchOptions, _logger);

        repositoryFetcher.EnsureRepositoryUpdated(_pathFormatStrategy, new GithubRepository(repositoryName.Owner, repositoryName.Name));
    }

    public void PushCommitToRemote(string repositoryLocalPath, string branchName)
    {
        using var repo = new Repository(repositoryLocalPath);

        Remote? remote = repo.Network.Remotes["origin"];
        string pushRefSpec = $"refs/heads/{branchName}";

        var pushOptions = new PushOptions
        {
            CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials() { Username = _credential.GithubUsername, Password = _credential.GithubToken }
        };

        repo.Network.Push(remote, [pushRefSpec], pushOptions);
    }

    public void CreatePullRequest(GithubRepositoryName repositoryName, string message)
    {
        repositoryName.ThrowIfNull();
        message.ThrowIfNull();

        string targetPath = _pathFormatStrategy.GetPathToRepository(new GithubRepository(repositoryName.Owner, repositoryName.Name));
        using (PowerShellPathChangeContext.TemporaryChangeCurrentDirectory(_powerShellAccessor, targetPath))
        {
            _powerShellAccessor.ExecuteAndGet(new PowerShellQuery($"gh pr create --title \"Fix warnings from Zeya\" --body \"{message}\""));
        }
    }

    public bool DeleteBranchOnMerge(GithubRepositoryName githubRepositoryName)
    {
        githubRepositoryName.ThrowIfNull();

        var repositoryInfo = _gitHubClient.Repository.Get(githubRepositoryName.Owner, githubRepositoryName.Name).Result;
        return repositoryInfo.DeleteBranchOnMerge ?? false;
    }

    public RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepositoryName githubRepositoryName, string branch)
    {
        githubRepositoryName.ThrowIfNull();

        try
        {
            BranchProtectionSettings repositoryBranchProtection = _gitHubClient.Repository.Branch.GetBranchProtection(githubRepositoryName.Owner, githubRepositoryName.Name, branch).Result;
            return new RepositoryBranchProtection(repositoryBranchProtection.RequiredPullRequestReviews is not null, repositoryBranchProtection.RequiredConversationResolution?.Enabled ?? false);
        }
        catch (Exception e)
        {
            // TODO: rework this. Possible exception: NotFound, Forbidden (for private repo)
            _logger.LogWarning("Failed to get branch protection info for {Repository}: {Message}", githubRepositoryName.FullName, e.Message);
            return new RepositoryBranchProtection(false, false);
        }
    }
}