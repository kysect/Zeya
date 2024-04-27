using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.OrganizationsSync.LocalStoragePathFactories;
using Kysect.GithubUtils.Replication.OrganizationsSync.RepositoryDiscovering;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.Zeya.GithubIntegration.Abstraction;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Octokit;
using Repository = LibGit2Sharp.Repository;

namespace Kysect.Zeya.GithubIntegration;

public class GithubIntegrationService : IGithubIntegrationService
{
    private readonly IRepositoryFetcher _repositoryFetcher;
    private readonly GithubIntegrationCredential _credential;
    private readonly IGitHubClient _gitHubClient;
    private readonly ILocalStoragePathFactory _pathFormatStrategy;
    private readonly ILogger _logger;

    public GithubIntegrationService(
        GithubIntegrationCredential credential,
        IGitHubClient gitHubClient,
        ILocalStoragePathFactory pathFormatStrategy,
        IRepositoryFetcher repositoryFetcher,
        ILogger<GithubIntegrationService> logger)
    {
        _repositoryFetcher = repositoryFetcher;
        _credential = credential.ThrowIfNull();

        _pathFormatStrategy = pathFormatStrategy.ThrowIfNull();
        _gitHubClient = gitHubClient.ThrowIfNull();
        _logger = logger.ThrowIfNull();
    }

    public async Task<IReadOnlyCollection<GithubRepositoryName>> GetOrganizationRepositories(string organization)
    {
        var gitHubRepositoryDiscoveryService = new GitHubRepositoryDiscoveryService(_gitHubClient);
        IReadOnlyList<GithubRepositoryBranch> githubRepositoryBranches = await gitHubRepositoryDiscoveryService.GetRepositories(organization);

        return githubRepositoryBranches
            .Select(r => new GithubRepositoryName(r.Owner, r.Name))
            .ToList();
    }

    public void CloneOrUpdate(GithubRepositoryName repositoryName)
    {
        repositoryName.ThrowIfNull();

        var repository = new GithubRepository(repositoryName.Owner, repositoryName.Name);
        string pathToRepository = _pathFormatStrategy.GetPathToRepository(repository);
        _repositoryFetcher.EnsureRepositoryUpdated(pathToRepository, repository);
    }

    public void PushCommitToRemote(string repositoryLocalPath, string branchName)
    {
        using var repo = new Repository(repositoryLocalPath);

        Remote? remote = repo.Network.Remotes["origin"];
        string pushRefSpec = $"refs/heads/{branchName}";

        var pushOptions = new PushOptions
        {
            CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials() { Username = _credential.GithubUsername, Password = _credential.GithubToken }
        };

        repo.Network.Push(remote, [pushRefSpec], pushOptions);
    }

    public async Task CreatePullRequest(GithubRepositoryName repositoryName, string message, string pullRequestTitle, string branch, string baseBranch)
    {
        repositoryName.ThrowIfNull();
        message.ThrowIfNull();

        // TODO: return PR info
        PullRequest pullRequest = await _gitHubClient.Repository.PullRequest.Create(
            repositoryName.Owner,
            repositoryName.Name,
            new NewPullRequest(pullRequestTitle, branch, baseBranch) { Body = message });
    }

    public async Task<bool> DeleteBranchOnMerge(GithubRepositoryName githubRepositoryName)
    {
        githubRepositoryName.ThrowIfNull();

        var repositoryInfo = await _gitHubClient.Repository.Get(githubRepositoryName.Owner, githubRepositoryName.Name);
        return repositoryInfo.DeleteBranchOnMerge ?? false;
    }

    public RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepositoryName githubRepositoryName, string branch)
    {
        githubRepositoryName.ThrowIfNull();

        try
        {
            BranchProtectionSettings repositoryBranchProtection = _gitHubClient.Repository.Branch.GetBranchProtection(githubRepositoryName.Owner, githubRepositoryName.Name, branch).Result;
            return new RepositoryBranchProtection(repositoryBranchProtection.RequiredPullRequestReviews is not null, repositoryBranchProtection.RequiredConversationResolution?.Enabled ?? false);
        }
        catch (Exception e)
        {
            // TODO: rework this. Possible exception: NotFound, Forbidden (for private repo)
            _logger.LogWarning("Failed to get branch protection info for {Repository}: {Message}", githubRepositoryName.FullName, e.Message);
            return new RepositoryBranchProtection(false, false);
        }
    }
}