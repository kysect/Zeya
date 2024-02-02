using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Branch = LibGit2Sharp.Branch;
using Repository = LibGit2Sharp.Repository;
using Signature = LibGit2Sharp.Signature;

namespace Kysect.Zeya.GithubIntegration;

public class GitIntegrationService : IGitIntegrationService
{
    private readonly GithubIntegrationOptions _githubIntegrationOptions;
    private readonly ILocalStoragePathFactory _pathFormatStrategy;
    private readonly ILogger _logger;

    public GitIntegrationService(IOptions<GithubIntegrationOptions> githubIntegrationOptions, ILocalStoragePathFactory pathFormatStrategy, ILogger logger)
    {
        githubIntegrationOptions.ThrowIfNull();

        _githubIntegrationOptions = githubIntegrationOptions.Value;
        _pathFormatStrategy = pathFormatStrategy.ThrowIfNull();
        _logger = logger.ThrowIfNull();
    }

    public void CloneOrUpdate(GithubRepository repository)
    {
        repository.ThrowIfNull();

        var repositoryFetchOptions = new RepositoryFetchOptions(_githubIntegrationOptions.GithubUsername, _githubIntegrationOptions.GithubToken);
        var repositoryFetcher = new RepositoryFetcher(repositoryFetchOptions, _logger);

        repositoryFetcher.EnsureRepositoryUpdated(_pathFormatStrategy, new GithubUtils.Models.GithubRepository(repository.Owner, repository.Name));
    }

    public void CreateFixBranch(GithubRepository repository, string branchName)
    {
        repository.ThrowIfNull();

        var repositoryFetchOptions = new RepositoryFetchOptions(_githubIntegrationOptions.GithubUsername, _githubIntegrationOptions.GithubToken);

        repository.ThrowIfNull();

        string targetPath = _pathFormatStrategy.GetPathToRepository(new GithubUtils.Models.GithubRepository(repository.Owner, repository.Name));
        using var repo = new Repository(targetPath);
        // TODO: validate that branch is not exists
        Branch branch = repo.CreateBranch(branchName);
        Branch currentBranch = Commands.Checkout(repo, branch);
    }

    public void CreateCommitWithFix(GithubRepository repository, string commitMessage)
    {
        repository.ThrowIfNull();

        string targetPath = _pathFormatStrategy.GetPathToRepository(new GithubUtils.Models.GithubRepository(repository.Owner, repository.Name));
        using var repo = new Repository(targetPath);
        Commands.Stage(repo, "*");
        Configuration config = repo.Config;
        Signature author = config.BuildSignature(DateTimeOffset.Now);
        repo.Commit(commitMessage, author, author);
    }

    public void PushCommitToRemote(GithubRepository repository, string branchName)
    {
        repository.ThrowIfNull();

        string targetPath = _pathFormatStrategy.GetPathToRepository(new GithubUtils.Models.GithubRepository(repository.Owner, repository.Name));
        using var repo = new Repository(targetPath);

        Remote? remote = repo.Network.Remotes["origin"];
        string pushRefSpec = $"refs/heads/{branchName}";

        var pushOptions = new PushOptions
        {
            CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials() { Username = _githubIntegrationOptions.GithubUsername, Password = _githubIntegrationOptions.GithubToken }
        };

        repo.Network.Push(remote, [pushRefSpec], pushOptions);
    }
}