namespace Kysect.Zeya.GithubIntegration.Abstraction;

public interface IGithubIntegrationService
{
    Task<IReadOnlyCollection<GithubRepositoryName>> GetOrganizationRepositories(string organization);

    void CloneOrUpdate(GithubRepositoryName repositoryName);
    void PushCommitToRemote(string repositoryLocalPath, string branchName);
    Task CreatePullRequest(GithubRepositoryName repositoryName, string message, string pullRequestTitle, string branch, string baseBranch);
    Task<bool> DeleteBranchOnMerge(GithubRepositoryName githubRepositoryName);
    RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepositoryName githubRepositoryName, string branch);
}