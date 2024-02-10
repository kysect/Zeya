using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Kysect.Zeya.GitIntegration.Abstraction;

namespace Kysect.Zeya.GithubIntegration.Abstraction.Contracts;

public interface IGithubIntegrationService
{
    void CloneOrUpdate(GithubRepository repository);
    void PushCommitToRemote(IClonedRepository repository, string branchName);
    void CreatePullRequest(GithubRepository repository, string message);
    bool DeleteBranchOnMerge(GithubRepository githubRepository);
    RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepository githubRepository, string branch);
}