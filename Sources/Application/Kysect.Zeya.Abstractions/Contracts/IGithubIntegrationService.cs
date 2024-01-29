using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IGithubIntegrationService
{
    void CloneOrUpdate(GithubRepository repository);
    void CreateFixBranch(GithubRepository repository, string branchName);
    void CreateCommitWithFix(GithubRepository repository, string commitMessage);
    void PushCommitToRemote(GithubRepository repository, string branchName);
    void CreatePullRequest(GithubRepository repository, string message);

    bool DeleteBranchOnMerge(GithubRepository githubRepository);
    RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepository githubRepository, string branch);
}