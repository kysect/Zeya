using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.GithubIntegration;

public interface IGithubIntegrationService
{
    void CloneOrUpdate(GithubRepository repository);
    void CreateFixBranch(GithubRepository repository, string branchName);
    void CreateCommitWithFix(GithubRepository repository, string commitMessage);
    void PushCommitToRemote(GithubRepository repository, string branchName);
    void CreatePullRequest(ClonedRepository repository, string message);
}