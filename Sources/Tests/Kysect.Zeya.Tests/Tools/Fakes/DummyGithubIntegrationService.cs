using Kysect.Zeya.GithubIntegration.Abstraction;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public class DummyGithubIntegrationService : IGithubIntegrationService
{
    public Task CreatePullRequest(GithubRepositoryName repositoryName, string message, string pullRequestTitle, string branch, string baseBranch)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteBranchOnMerge(GithubRepositoryName githubRepositoryName)
    {
        throw new NotImplementedException();
    }

    public RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepositoryName githubRepositoryName, string branch)
    {
        throw new NotImplementedException();
    }
}