using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

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

    public IReadOnlyCollection<LocalGithubRepository> GetGithubOrganizationRepositories(string organization, IReadOnlyCollection<string> excludedRepositories)
    {
        HashSet<string> skipList = excludedRepositories.ToHashSet();

        IReadOnlyCollection<GithubRepositoryName> githubRepositories = _githubIntegrationService
            .GetOrganizationRepositories(organization)
            .Where(repository => !skipList.Contains(repository.Name))
            .ToList();

        List<LocalGithubRepository> result = githubRepositories
            .Select(CreateGithubRepositoryAccessor)
            .ToList();

        return result;
    }

    public LocalGithubRepository GetGithubRepository(string owner, string repository)
    {
        return CreateGithubRepositoryAccessor(new GithubRepositoryName(owner, repository));
    }

    public ILocalRepository GetLocalRepository(string path)
    {
        return new LocalRepository(path, _fileSystem);
    }

    private LocalGithubRepository CreateGithubRepositoryAccessor(GithubRepositoryName githubRepositoryName)
    {
        _logger.LogInformation("Loading repository {Repository}", githubRepositoryName.FullName);
        _githubIntegrationService.CloneOrUpdate(githubRepositoryName);
        string repositoryRootPath = _localStoragePathFactory.GetPathToRepository(new GithubRepository(githubRepositoryName.Owner, githubRepositoryName.Name));
        return new LocalGithubRepository(githubRepositoryName, repositoryRootPath, _fileSystem);
    }
}