using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using LibGit2Sharp;
using Branch = LibGit2Sharp.Branch;
using Repository = LibGit2Sharp.Repository;
using Signature = LibGit2Sharp.Signature;

namespace Kysect.Zeya.GitIntegration;

public class GitIntegrationService : IGitIntegrationService
{
    private readonly GitCommitAuthor? _commitAuthor;

    public GitIntegrationService(GitCommitAuthor? commitAuthor)
    {
        _commitAuthor = commitAuthor;
    }

    public void CreateFixBranch(ILocalRepository repository, string branchName)
    {
        repository.ThrowIfNull();

        repository.ThrowIfNull();

        string targetPath = repository.FileSystem.GetFullPath();
        using var repo = new Repository(targetPath);
        // TODO: validate that branch is not exists
        Branch branch = repo.CreateBranch(branchName);
        Branch currentBranch = Commands.Checkout(repo, branch);
    }

    public void CreateCommitWithFix(ILocalRepository repository, string commitMessage)
    {
        repository.ThrowIfNull();

        string targetPath = repository.FileSystem.GetFullPath();
        using var repo = new Repository(targetPath);
        Commands.Stage(repo, "*");

        Signature author = _commitAuthor is not null
            ? new Signature(_commitAuthor.GithubUsername, _commitAuthor.GithubMail, DateTimeOffset.Now)
            : repo.Config.BuildSignature(DateTimeOffset.UtcNow);

        repo.Commit(commitMessage, author, author);
    }
}