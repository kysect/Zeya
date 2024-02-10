using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.OrganizationsSync.RepositoryDiscovering;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System.IO.Abstractions;
using ClonedGithubRepository = Kysect.Zeya.GithubIntegration.Abstraction.ClonedGithubRepository;

namespace Kysect.Zeya.IntegrationManager;

public class GithubRepositoryProvider : IGithubRepositoryProvider
{
    private readonly IFileSystem _fileSystem;
    private readonly IGitHubClient _gitHub;
    private readonly IGithubIntegrationService _githubIntegrationService;
    private readonly ILocalStoragePathFactory _localStoragePathFactory;
    private readonly GithubIntegrationOptions _githubIntegrationOptions;
    private readonly ILogger _logger;

    public GithubRepositoryProvider(
        IGitHubClient gitHub,
        IFileSystem fileSystem,
        IOptions<GithubIntegrationOptions> githubIntegrationOptions,
        ILogger logger,
        IGithubIntegrationService githubIntegrationService,
        ILocalStoragePathFactory localStoragePathFactory)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _githubIntegrationService = githubIntegrationService;
        _localStoragePathFactory = localStoragePathFactory;
        _gitHub = gitHub.ThrowIfNull();

        githubIntegrationOptions.ThrowIfNull();
        _githubIntegrationOptions = githubIntegrationOptions.Value;
    }

    public IReadOnlyCollection<ClonedGithubRepository> GetGithubOrganizationRepositories(string organization)
    {
        IReadOnlyCollection<GithubRepositoryName> githubRepositories = GetAllInner(organization).Result;

        var result = githubRepositories
            .Select(CreateGithubRepositoryAccessor)
            .ToList();

        return result;
    }

    public ClonedGithubRepository GetGithubRepository(string owner, string repository)
    {
        return CreateGithubRepositoryAccessor(new GithubRepositoryName(owner, repository));
    }

    public IClonedRepository GetLocalRepository(string path)
    {
        return new ClonedRepository(path, _fileSystem);
    }

    private async Task<IReadOnlyCollection<GithubRepositoryName>> GetAllInner(string organization)
    {
        var result = new List<GithubRepositoryName>();

        var skipList = _githubIntegrationOptions.ExcludedRepositories.ToHashSet();
        var gitHubRepositoryDiscoveryService = new GitHubRepositoryDiscoveryService(_gitHub);
        foreach (GithubRepositoryBranch repository in await gitHubRepositoryDiscoveryService.GetRepositories(organization))
        {
            if (!skipList.Contains(repository.Name))
                result.Add(new GithubRepositoryName(organization, repository.Name));
        }

        return result;
    }

    private ClonedGithubRepository CreateGithubRepositoryAccessor(GithubRepositoryName githubRepositoryName)
    {
        _logger.LogInformation("Loading repository {Repository}", githubRepositoryName.FullName);
        _githubIntegrationService.CloneOrUpdate(githubRepositoryName);
        string repositoryRootPath = _localStoragePathFactory.GetPathToRepository(new GithubRepository(githubRepositoryName.Owner, githubRepositoryName.Name));
        return new ClonedGithubRepository(githubRepositoryName, repositoryRootPath, _fileSystem);
    }
}