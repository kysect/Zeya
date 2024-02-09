using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IGitIntegrationService
{
    void CloneOrUpdate(GithubRepository repository);
    void CreateFixBranch(IClonedRepository repository, string branchName);
    void CreateCommitWithFix(IClonedRepository repository, string commitMessage);
    void PushCommitToRemote(IClonedRepository repository, string branchName);
}