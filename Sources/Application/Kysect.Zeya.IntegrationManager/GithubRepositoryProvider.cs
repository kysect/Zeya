using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using ClonedGithubRepository = Kysect.Zeya.GithubIntegration.Abstraction.ClonedGithubRepository;

namespace Kysect.Zeya.IntegrationManager;

public class GithubRepositoryProvider : IGithubRepositoryProvider
{
    private readonly IFileSystem _fileSystem;
    private readonly IGithubIntegrationService _githubIntegrationService;
    private readonly ILocalStoragePathFactory _localStoragePathFactory;
    private readonly ILogger _logger;

    public GithubRepositoryProvider(
        IFileSystem fileSystem,
        ILogger logger,
        IGithubIntegrationService githubIntegrationService,
        ILocalStoragePathFactory localStoragePathFactory)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _githubIntegrationService = githubIntegrationService;
        _localStoragePathFactory = localStoragePathFactory;
    }

    public IReadOnlyCollection<ClonedGithubRepository> GetGithubOrganizationRepositories(string organization, IReadOnlyCollection<string> excludedRepositories)
    {
        HashSet<string> skipList = excludedRepositories.ToHashSet();

        IReadOnlyCollection<GithubRepositoryName> githubRepositories = _githubIntegrationService
            .GetOrganizationRepositories(organization)
            .Where(repository => !skipList.Contains(repository.Name))
            .ToList();

        List<ClonedGithubRepository> result = githubRepositories
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

    private ClonedGithubRepository CreateGithubRepositoryAccessor(GithubRepositoryName githubRepositoryName)
    {
        _logger.LogInformation("Loading repository {Repository}", githubRepositoryName.FullName);
        _githubIntegrationService.CloneOrUpdate(githubRepositoryName);
        string repositoryRootPath = _localStoragePathFactory.GetPathToRepository(new GithubRepository(githubRepositoryName.Owner, githubRepositoryName.Name));
        return new ClonedGithubRepository(githubRepositoryName, repositoryRootPath, _fileSystem);
    }
}