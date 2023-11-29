using Kysect.GithubUtils.RepositorySync;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.GithubIntegration;

public class GithubIntegrationService : IGithubIntegrationService
{
    private readonly GithubIntegrationOptions _githubIntegrationOptions;
    private readonly IPathFormatStrategy _pathFormatStrategy;
    private readonly ILogger _logger;

    public GithubIntegrationService(GithubIntegrationOptions githubIntegrationOptions, IPathFormatStrategy pathFormatStrategy, ILogger logger)
    {
        _githubIntegrationOptions = githubIntegrationOptions;
        _pathFormatStrategy = pathFormatStrategy;
        _logger = logger;
    }

    public void CloneOrUpdate(GithubRepository repository)
    {
        var repositoryFetchOptions = new RepositoryFetchOptions(_githubIntegrationOptions.GithubUsername, _githubIntegrationOptions.GithubToken);
        var repositoryFetcher = new RepositoryFetcher(repositoryFetchOptions, _logger);


        repositoryFetcher.EnsureRepositoryUpdated(
            _pathFormatStrategy,
            new GithubUtils.RepositorySync.Models.GithubRepository(repository.Owner, repository.Name));
    }
}