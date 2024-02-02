using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IGitIntegrationService
{
    void CloneOrUpdate(GithubRepository repository);
    void CreateFixBranch(GithubRepository repository, string branchName);
    void CreateCommitWithFix(GithubRepository repository, string commitMessage);
    void PushCommitToRemote(GithubRepository repository, string branchName);
}