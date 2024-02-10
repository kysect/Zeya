using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Kysect.Zeya.GitIntegration.Abstraction;

namespace Kysect.Zeya.GithubIntegration.Abstraction.Contracts;

public interface IGithubIntegrationService
{
    void CloneOrUpdate(GithubRepositoryName repositoryName);
    void PushCommitToRemote(IClonedRepository repository, string branchName);
    void CreatePullRequest(GithubRepositoryName repositoryName, string message);
    bool DeleteBranchOnMerge(GithubRepositoryName githubRepositoryName);
    RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepositoryName githubRepositoryName, string branch);
}