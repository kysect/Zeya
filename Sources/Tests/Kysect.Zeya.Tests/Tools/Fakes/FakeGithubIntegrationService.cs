﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public class FakeGithubIntegrationService : IGithubIntegrationService
{
    private readonly ILocalStoragePathFactory _localStoragePathFactory;
    private readonly ILogger _logger;
    private readonly GithubIntegrationCredential _options;
    public RepositoryBranchProtection RepositoryBranchProtection { get; set; }
    public bool BranchProtectionEnabled { get; set; }

    public FakeGithubIntegrationService(GithubIntegrationCredential githubIntegrationOptions, ILocalStoragePathFactory localStoragePathFactory, ILogger logger)
    {
        _options = githubIntegrationOptions.ThrowIfNull();
        _localStoragePathFactory = localStoragePathFactory;
        _logger = logger;
        RepositoryBranchProtection = new RepositoryBranchProtection(false, false);
    }

    public Task<IReadOnlyCollection<GithubRepositoryName>> GetOrganizationRepositories(string organization)
    {
        throw new NotImplementedException();
    }

    public void CloneOrUpdate(GithubRepositoryName repositoryName)
    {
        repositoryName.ThrowIfNull();

        var repositoryFetchOptions = new RepositoryFetchOptions(_options.GithubUsername, _options.GithubToken);
        var repositoryFetcher = new RepositoryFetcher(repositoryFetchOptions, _logger);

        repositoryFetcher.EnsureRepositoryUpdated(_localStoragePathFactory, new GithubUtils.Models.GithubRepository(repositoryName.Owner, repositoryName.Name));
    }

    public void PushCommitToRemote(string repositoryLocalPath, string branchName)
    {
        throw new NotImplementedException();
    }

    public Task CreateFixBranch(GithubRepositoryName repositoryName, string branchName)
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

    public Task CreatePullRequest(GithubRepositoryName repositoryName, string message, string pullRequestTitle, string branch, string baseBranch)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteBranchOnMerge(GithubRepositoryName githubRepositoryName)
    {
        return Task.FromResult(BranchProtectionEnabled);
    }

    public RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepositoryName githubRepositoryName, string branch)
    {
        return RepositoryBranchProtection;
    }
}