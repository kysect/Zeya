namespace Kysect.Zeya.GitIntegration.Abstraction;

public interface IGitIntegrationService
{
    void PushCommitToRemote(string repositoryLocalPath, string branchName, GitRepositoryCredential gitRepositoryCredential);
    string GetDiff(string repositoryLocalPath);
    void CreateFixBranch(string repositoryLocalPath, string branchName);
    void CreateCommitWithFix(string repositoryLocalPath, string commitMessage);
}