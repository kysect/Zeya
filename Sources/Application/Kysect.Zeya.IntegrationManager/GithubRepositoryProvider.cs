using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.OrganizationsSync.RepositoryDiscovering;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction.Contracts;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Kysect.Zeya.GitIntegration;
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
    private readonly GithubIntegrationOptions _githubIntegrationOptions;
    private readonly IClonedRepositoryFactory<ClonedGithubRepository> _repositoryFactory;
    private readonly ILogger _logger;

    public GithubRepositoryProvider(
        IGitHubClient gitHub,
        IFileSystem fileSystem,
        IOptions<GithubIntegrationOptions> githubIntegrationOptions,
        IClonedRepositoryFactory<ClonedGithubRepository> repositoryFactory,
        ILogger logger,
        IGithubIntegrationService githubIntegrationService)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _githubIntegrationService = githubIntegrationService;
        _gitHub = gitHub.ThrowIfNull();
        _repositoryFactory = repositoryFactory.ThrowIfNull();

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
        return _repositoryFactory.Create(githubRepositoryName);
    }
}