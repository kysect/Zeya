using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kysect.Zeya.Tests.Fakes;

public class FakeGithubIntegrationService : IGithubIntegrationService
{
    private readonly ILocalStoragePathFactory _localStoragePathFactory;
    private readonly ILogger _logger;
    private readonly GithubIntegrationOptions _options;
    public RepositoryBranchProtection RepositoryBranchProtection { get; set; }
    public bool BranchProtectionEnabled { get; set; }

    public FakeGithubIntegrationService(IOptions<GithubIntegrationOptions> githubIntegrationOptions, ILocalStoragePathFactory localStoragePathFactory, ILogger logger)
    {
        githubIntegrationOptions.ThrowIfNull();

        _options = githubIntegrationOptions.Value;
        _localStoragePathFactory = localStoragePathFactory;
        _logger = logger;
        RepositoryBranchProtection = new RepositoryBranchProtection(false, false);
    }

    public void CloneOrUpdate(GithubRepositoryName repositoryName)
    {
        repositoryName.ThrowIfNull();

        var repositoryFetchOptions = new RepositoryFetchOptions(_options.GithubUsername, _options.GithubToken);
        var repositoryFetcher = new RepositoryFetcher(repositoryFetchOptions, _logger);

        repositoryFetcher.EnsureRepositoryUpdated(_localStoragePathFactory, new GithubUtils.Models.GithubRepository(repositoryName.Owner, repositoryName.Name));
    }

    public void PushCommitToRemote(IClonedRepository repository, string branchName)
    {
        throw new NotImplementedException();
    }

    public void CreateFixBranch(GithubRepositoryName repositoryName, string branchName)
    {
        throw new NotImplementedException();
    }

    public void CreateCommitWithFix(GithubRepositoryName repositoryName, string commitMessage)
    {
        throw new NotImplementedException();
    }

    public void PushCommitToRemote(GithubRepositoryName repositoryName, string branchName)
    {
        throw new NotImplementedException();
    }

    public void CreatePullRequest(GithubRepositoryName repositoryName, string message)
    {
        throw new NotImplementedException();
    }

    public bool DeleteBranchOnMerge(GithubRepositoryName githubRepositoryName)
    {
        return BranchProtectionEnabled;
    }

    public RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepositoryName githubRepositoryName, string branch)
    {
        return RepositoryBranchProtection;
    }
}