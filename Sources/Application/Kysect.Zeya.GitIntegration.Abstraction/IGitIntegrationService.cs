namespace Kysect.Zeya.GitIntegration.Abstraction;

public interface IGitIntegrationService
{
    void CreateFixBranch(string repositoryLocalPath, string branchName);
    void CreateCommitWithFix(string repositoryLocalPath, string commitMessage);
}