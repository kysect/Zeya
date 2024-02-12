using Kysect.Zeya.LocalRepositoryAccess;

namespace Kysect.Zeya.GitIntegration.Abstraction;

public interface IGitIntegrationService
{
    void CreateFixBranch(ILocalRepository repository, string branchName);
    void CreateCommitWithFix(ILocalRepository repository, string commitMessage);
}