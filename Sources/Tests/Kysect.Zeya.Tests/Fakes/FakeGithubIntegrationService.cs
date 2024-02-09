using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction.Contracts;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;
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

    public void CloneOrUpdate(GithubRepository repository)
    {
        repository.ThrowIfNull();

        var repositoryFetchOptions = new RepositoryFetchOptions(_options.GithubUsername, _options.GithubToken);
        var repositoryFetcher = new RepositoryFetcher(repositoryFetchOptions, _logger);

        repositoryFetcher.EnsureRepositoryUpdated(_localStoragePathFactory, new GithubUtils.Models.GithubRepository(repository.Owner, repository.Name));
    }

    public void CreateFixBranch(GithubRepository repository, string branchName)
    {
        throw new NotImplementedException();
    }

    public void CreateCommitWithFix(GithubRepository repository, string commitMessage)
    {
        throw new NotImplementedException();
    }

    public void PushCommitToRemote(GithubRepository repository, string branchName)
    {
        throw new NotImplementedException();
    }

    public void CreatePullRequest(GithubRepository repository, string message)
    {
        throw new NotImplementedException();
    }

    public bool DeleteBranchOnMerge(GithubRepository githubRepository)
    {
        return BranchProtectionEnabled;
    }

    public RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepository githubRepository, string branch)
    {
        return RepositoryBranchProtection;
    }
}