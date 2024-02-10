using Kysect.Zeya.GitIntegration.Abstraction;

namespace Kysect.Zeya.GithubIntegration.Abstraction;

public interface IGithubIntegrationService
{
    IReadOnlyCollection<GithubRepositoryName> GetOrganizationRepositories(string organization);

    void CloneOrUpdate(GithubRepositoryName repositoryName);
    void PushCommitToRemote(IClonedRepository repository, string branchName);
    void CreatePullRequest(GithubRepositoryName repositoryName, string message);
    bool DeleteBranchOnMerge(GithubRepositoryName githubRepositoryName);
    RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepositoryName githubRepositoryName, string branch);
}