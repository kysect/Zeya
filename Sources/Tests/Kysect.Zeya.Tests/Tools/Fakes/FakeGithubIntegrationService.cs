using Kysect.Zeya.GithubIntegration.Abstraction;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public class FakeGithubIntegrationService : IGithubIntegrationService
{
    public RepositoryBranchProtection RepositoryBranchProtection { get; set; }
    public bool BranchProtectionEnabled { get; set; }

    public FakeGithubIntegrationService()
    {
        RepositoryBranchProtection = new RepositoryBranchProtection(false, false);
    }

    public void PushCommitToRemote(string repositoryLocalPath, string branchName)
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