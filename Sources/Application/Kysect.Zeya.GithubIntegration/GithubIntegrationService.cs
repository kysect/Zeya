﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.PowerShellRunner.Abstractions.Accessors;
using Kysect.PowerShellRunner.Abstractions.Queries;
using Kysect.PowerShellRunner.Executions;
using Kysect.PowerShellRunner.Tools;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GithubRepository = Kysect.Zeya.Abstractions.Models.GithubRepository;
using Repository = LibGit2Sharp.Repository;
using Signature = LibGit2Sharp.Signature;

namespace Kysect.Zeya.GithubIntegration;

public class GithubIntegrationService : IGithubIntegrationService
{
    private readonly IPowerShellAccessor _powerShellAccessor;
    private readonly GithubIntegrationOptions _githubIntegrationOptions;
    private readonly ILocalStoragePathFactory _pathFormatStrategy;
    private readonly ILogger _logger;

    public GithubIntegrationService(IOptions<GithubIntegrationOptions> githubIntegrationOptions, ILocalStoragePathFactory pathFormatStrategy, IPowerShellAccessor powerShellAccessor, ILogger logger)
    {
        _powerShellAccessor = powerShellAccessor;
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

        // TODO: use default branch from repository
        repositoryFetcher.Checkout(_pathFormatStrategy, new GithubRepositoryBranch(repository.Owner, repository.Name, "master"));
    }

    public void CreateFixBranch(GithubRepository repository)
    {
        repository.ThrowIfNull();

        var repositoryFetchOptions = new RepositoryFetchOptions(_githubIntegrationOptions.GithubUsername, _githubIntegrationOptions.GithubToken);
        var repositoryFetcher = new RepositoryFetcher(repositoryFetchOptions, _logger);

        repository.ThrowIfNull();

        string targetPath = _pathFormatStrategy.GetPathToRepository(new GithubUtils.Models.GithubRepository(repository.Owner, repository.Name));
        using var repo = new Repository(targetPath);
        // TODO: move constants

        Branch branch = repo.CreateBranch("zeya/fixer");
        Branch currentBranch = Commands.Checkout(repo, branch);
    }

    public void CreateCommitWithFix(GithubRepository repository)
    {
        repository.ThrowIfNull();

        string targetPath = _pathFormatStrategy.GetPathToRepository(new GithubUtils.Models.GithubRepository(repository.Owner, repository.Name));
        using var repo = new Repository(targetPath);
        Commands.Stage(repo, "*");
        // TODO: remove this hardcoded values
        Configuration config = repo.Config;
        Signature author = config.BuildSignature(DateTimeOffset.Now);
        repo.Commit("Apply Zeya fixes", author, author);
    }

    public void PushCommitToRemote(GithubRepository repository)
    {
        repository.ThrowIfNull();

        string targetPath = _pathFormatStrategy.GetPathToRepository(new GithubUtils.Models.GithubRepository(repository.Owner, repository.Name));
        using var repo = new Repository(targetPath);

        Remote? remote = repo.Network.Remotes["origin"];
        string pushRefSpec = @"refs/heads/zeya/fixer";

        var pushOptions = new PushOptions
        {
            CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials() { Username = _githubIntegrationOptions.GithubUsername, Password = _githubIntegrationOptions.GithubToken }
        };

        repo.Network.Push(remote, [pushRefSpec], pushOptions);
    }

    public void CreatePullRequest(ClonedRepository repository, string message)
    {
        repository.ThrowIfNull();
        message.ThrowIfNull();

        using (PowerShellPathChangeContext.TemporaryChangeCurrentDirectory(_powerShellAccessor, repository.GetFullPath()))
        {
            _powerShellAccessor.ExecuteAndGet(new PowerShellQuery($"gh pr create --title \"Fix warnings from Zeya\" --body \"{message}\""));
        }
    }
}