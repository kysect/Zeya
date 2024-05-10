using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.Zeya.GitIntegration.Abstraction;
using LibGit2Sharp;

namespace Kysect.Zeya.GitIntegration;

public class GitIntegrationService : IGitIntegrationService
{
    private readonly GitCommitAuthor? _commitAuthor;
    private readonly IRepositoryFetcher _fetcher;
    private readonly RemoteGitHostCredential _remoteGitHostCredential;

    public GitIntegrationService(
        GitCommitAuthor? commitAuthor,
        IRepositoryFetcher fetcher,
        RemoteGitHostCredential remoteGitHostCredential)
    {
        _commitAuthor = commitAuthor;
        _fetcher = fetcher;
        _remoteGitHostCredential = remoteGitHostCredential;
    }

    public string EnsureRepositoryUpdated(string targetPath, IRemoteGitRepository remoteRepository)
    {
        return _fetcher.EnsureRepositoryUpdated(targetPath, remoteRepository);
    }

    public void PushCommitToRemote(string repositoryLocalPath, string branchName)
    {
        using var repo = new Repository(repositoryLocalPath);

        Remote? remote = repo.Network.Remotes["origin"];
        string pushRefSpec = $"refs/heads/{branchName}";

        var pushOptions = new PushOptions
        {
            CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials { Username = _remoteGitHostCredential.Username, Password = _remoteGitHostCredential.Token }
        };

        repo.Network.Push(remote, [pushRefSpec], pushOptions);
    }

    public string GetDiff(string repositoryLocalPath)
    {
        using var repo = new Repository(repositoryLocalPath);

        Patch patch = repo.Diff.Compare<Patch>(["*"], includeUntracked: true);

        return patch.Content;
    }

    public void CreateFixBranch(string repositoryLocalPath, string branchName)
    {
        using var repo = new Repository(repositoryLocalPath);
        // TODO: validate that branch is not exists
        Branch branch = repo.CreateBranch(branchName);
        Branch currentBranch = Commands.Checkout(repo, branch);
    }

    public void CreateCommitWithFix(string repositoryLocalPath, string commitMessage)
    {
        using var repo = new Repository(repositoryLocalPath);
        Commands.Stage(repo, "*");

        Signature author = _commitAuthor is not null
            ? new Signature(_commitAuthor.GithubUsername, _commitAuthor.GithubMail, DateTimeOffset.Now)
            : repo.Config.BuildSignature(DateTimeOffset.UtcNow);

        repo.Commit(commitMessage, author, author);
    }
}