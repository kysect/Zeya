using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.Abstractions.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GithubRepository = Kysect.Zeya.Abstractions.Models.GithubRepository;

namespace Kysect.Zeya.GithubIntegration;

public class GithubIntegrationService : IGithubIntegrationService
{
    private readonly GithubIntegrationOptions _githubIntegrationOptions;
    private readonly ILocalStoragePathFactory _pathFormatStrategy;
    private readonly ILogger _logger;

    public GithubIntegrationService(IOptions<GithubIntegrationOptions> githubIntegrationOptions, ILocalStoragePathFactory pathFormatStrategy, ILogger logger)
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

        // TODO: use default branch from repository
        repositoryFetcher.Checkout(_pathFormatStrategy, new GithubRepositoryBranch(repository.Owner, repository.Name, "master"));
    }
}