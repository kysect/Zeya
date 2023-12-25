using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.GithubIntegration;

public interface IGithubIntegrationService
{
    void CloneOrUpdate(GithubRepository repository);
    void CreateFixBranch(GithubRepository repository);
    void CreateCommitWithFix(GithubRepository repository);
    void PushCommitToRemote(GithubRepository repository);
    void CreatePullRequest(ClonedRepository repository, string message);
}