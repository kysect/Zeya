using Kysect.Zeya.GitIntegration.Abstraction;
using LibGit2Sharp;

namespace Kysect.Zeya.GitIntegration;

public class GitIntegrationService : IGitIntegrationService
{
    private readonly GitCommitAuthor? _commitAuthor;

    public GitIntegrationService(GitCommitAuthor? commitAuthor)
    {
        _commitAuthor = commitAuthor;
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