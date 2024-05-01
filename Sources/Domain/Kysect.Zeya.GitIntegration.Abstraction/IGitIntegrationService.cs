using Kysect.GithubUtils.Models;

namespace Kysect.Zeya.GitIntegration.Abstraction;

public interface IGitIntegrationService
{
    string EnsureRepositoryUpdated(string targetPath, IRemoteGitRepository remoteRepository);
    void PushCommitToRemote(string repositoryLocalPath, string branchName);
    string GetDiff(string repositoryLocalPath);
    void CreateFixBranch(string repositoryLocalPath, string branchName);
    void CreateCommitWithFix(string repositoryLocalPath, string commitMessage);
}