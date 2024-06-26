﻿namespace Kysect.Zeya.GithubIntegration.Abstraction;

public interface IGithubIntegrationService
{
    Task CreatePullRequest(GithubRepositoryName repositoryName, string message, string pullRequestTitle, string branch, string baseBranch);
    Task<bool> DeleteBranchOnMerge(GithubRepositoryName githubRepositoryName);
    RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepositoryName githubRepositoryName, string branch);
}