namespace Kysect.Zeya.GitIntegration.Abstraction;

public interface IGitIntegrationService
{
    string GetDiff(string repositoryLocalPath);
    void CreateFixBranch(string repositoryLocalPath, string branchName);
    void CreateCommitWithFix(string repositoryLocalPath, string commitMessage);
}