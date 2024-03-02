namespace Kysect.Zeya.GithubIntegration.Abstraction;

public interface IGithubIntegrationService
{
    IReadOnlyCollection<GithubRepositoryName> GetOrganizationRepositories(string organization);

    void CloneOrUpdate(GithubRepositoryName repositoryName);
    void PushCommitToRemote(string repositoryLocalPath, string branchName);
    void CreatePullRequest(GithubRepositoryName repositoryName, string message, string pullRequestTitle, string branch, string baseBranch);
    bool DeleteBranchOnMerge(GithubRepositoryName githubRepositoryName);
    RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepositoryName githubRepositoryName, string branch);
}