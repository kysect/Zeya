namespace Kysect.Zeya.GitIntegration.Abstraction;

public interface IGitIntegrationService
{
    void PushCommitToRemote(string repositoryLocalPath, string branchName, GitRepositoryCredentialOptions gitRepositoryCredentialOptions);
    string GetDiff(string repositoryLocalPath);
    void CreateFixBranch(string repositoryLocalPath, string branchName);
    void CreateCommitWithFix(string repositoryLocalPath, string commitMessage);
}