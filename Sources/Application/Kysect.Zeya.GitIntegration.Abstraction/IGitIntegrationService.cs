namespace Kysect.Zeya.GitIntegration.Abstraction;

public interface IGitIntegrationService
{
    void CreateFixBranch(IClonedRepository repository, string branchName);
    void CreateCommitWithFix(IClonedRepository repository, string commitMessage);
}