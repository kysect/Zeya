using Kysect.GithubUtils.RepositorySync;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.GithubIntegration;

public class GithubIntegrationService : IGithubIntegrationService
{
    private readonly GithubIntegrationOptions _githubIntegrationOptions;
    private readonly ILogger _logger;

    public GithubIntegrationService(GithubIntegrationOptions githubIntegrationOptions, ILogger logger)
    {
        _githubIntegrationOptions = githubIntegrationOptions;
        _logger = logger;
    }

    public void CloneOrUpdate(GithubRepository repository)
    {
        var repositoryFetchOptions = new RepositoryFetchOptions(_githubIntegrationOptions.GithubUsername, _githubIntegrationOptions.GithubToken);
        var repositoryFetcher = new RepositoryFetcher(repositoryFetchOptions, _logger);


        repositoryFetcher.EnsureRepositoryUpdated(
            new UseOwnerAndRepoForFolderNameStrategy(_githubIntegrationOptions.CacheDirectoryPath),
            new GithubUtils.RepositorySync.Models.GithubRepository(repository.Owner, repository.Name));
    }
}