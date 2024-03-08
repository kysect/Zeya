using Kysect.Zeya.GithubIntegration.Abstraction;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public class DummyGithubIntegrationService : IGithubIntegrationService
{
    public IReadOnlyCollection<GithubRepositoryName> GetOrganizationRepositories(string organization)
    {
        throw new NotImplementedException();
    }

    public void CloneOrUpdate(GithubRepositoryName repositoryName)
    {
    }

    public void PushCommitToRemote(string repositoryLocalPath, string branchName)
    {
        throw new NotImplementedException();
    }

    public void CreatePullRequest(GithubRepositoryName repositoryName, string message, string pullRequestTitle, string branch, string baseBranch)
    {
        throw new NotImplementedException();
    }

    public bool DeleteBranchOnMerge(GithubRepositoryName githubRepositoryName)
    {
        throw new NotImplementedException();
    }

    public RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepositoryName githubRepositoryName, string branch)
    {
        throw new NotImplementedException();
    }
}