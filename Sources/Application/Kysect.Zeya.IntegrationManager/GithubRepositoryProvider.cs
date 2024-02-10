using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.OrganizationsSync.RepositoryDiscovering;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration.Abstraction.Contracts;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.GitIntegration.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System.IO.Abstractions;
using GithubRepository = Kysect.Zeya.GithubIntegration.Abstraction.Models.GithubRepository;

namespace Kysect.Zeya.IntegrationManager;

public class GithubRepositoryProvider : IGithubRepositoryProvider
{
    private readonly IFileSystem _fileSystem;
    private readonly IGitHubClient _gitHub;
    private readonly IGithubIntegrationService _githubIntegrationService;
    private readonly GithubIntegrationOptions _githubIntegrationOptions;
    private readonly IClonedRepositoryFactory<ClonedGithubRepositoryAccessor> _repositoryFactory;
    private readonly ILogger _logger;

    public GithubRepositoryProvider(
        IGitHubClient gitHub,
        IFileSystem fileSystem,
        IOptions<GithubIntegrationOptions> githubIntegrationOptions,
        IGitIntegrationService gitIntegrationService,
        IClonedRepositoryFactory<ClonedGithubRepositoryAccessor> repositoryFactory,
        ILogger logger,
        IGithubIntegrationService githubIntegrationService)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _githubIntegrationService = githubIntegrationService;
        _gitHub = gitHub.ThrowIfNull();
        gitIntegrationService.ThrowIfNull();
        _repositoryFactory = repositoryFactory.ThrowIfNull();
        githubIntegrationOptions.ThrowIfNull();

        _githubIntegrationOptions = githubIntegrationOptions.Value;
    }

    public IReadOnlyCollection<ClonedGithubRepositoryAccessor> GetGithubOrganizationRepositories(string organization)
    {
        IReadOnlyCollection<GithubRepository> githubRepositories = GetAllInner(organization).Result;

        var result = githubRepositories
            .Select(CreateGithubRepositoryAccessor)
            .ToList();

        return result;
    }

    public ClonedGithubRepositoryAccessor GetGithubRepository(string owner, string repository)
    {
        return CreateGithubRepositoryAccessor(new GithubRepository(owner, repository));
    }

    public IClonedRepository GetLocalRepository(string path)
    {
        return new ClonedRepositoryAccessor(path, _fileSystem);
    }

    private async Task<IReadOnlyCollection<GithubRepository>> GetAllInner(string organization)
    {
        var result = new List<GithubRepository>();

        var skipList = _githubIntegrationOptions.ExcludedRepositories.ToHashSet();
        var gitHubRepositoryDiscoveryService = new GitHubRepositoryDiscoveryService(_gitHub);
        foreach (GithubRepositoryBranch repository in await gitHubRepositoryDiscoveryService.GetRepositories(organization))
        {
            if (!skipList.Contains(repository.Name))
                result.Add(new GithubRepository(organization, repository.Name));
        }

        return result;
    }

    private ClonedGithubRepositoryAccessor CreateGithubRepositoryAccessor(GithubRepository githubRepository)
    {
        _logger.LogInformation("Loading repository {Repository}", githubRepository.FullName);
        _githubIntegrationService.CloneOrUpdate(githubRepository);
        return _repositoryFactory.Create(githubRepository);
    }
}