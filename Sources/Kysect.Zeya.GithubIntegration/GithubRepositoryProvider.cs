using Kysect.GithubUtils.RepositoryDiscovering;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Options;

namespace Kysect.Zeya.GithubIntegration;

public class GithubRepositoryProvider : IGithubRepositoryProvider
{
    private readonly GithubIntegrationOptions _githubIntegrationOptions;

    public GithubRepositoryProvider(IOptions<GithubIntegrationOptions> githubIntegrationOptions)
    {
        _githubIntegrationOptions = githubIntegrationOptions.Value;
    }

    public IReadOnlyCollection<GithubRepository> GetAll()
    {
        return GetAllInner().Result;
    }

    private async Task<IReadOnlyCollection<GithubRepository>> GetAllInner()
    {
        var result = new List<GithubRepository>();

        var skipList = _githubIntegrationOptions.ExcludedRepositories.ToHashSet();
        var discoveryService = new GitHubRepositoryDiscoveryService(_githubIntegrationOptions.GithubToken);
        await foreach (var repository in discoveryService.TryDiscover(_githubIntegrationOptions.IncludedOrganization))
        {
            if (!skipList.Contains(repository.Name))
            {
                result.Add(new GithubRepository(_githubIntegrationOptions.IncludedOrganization, repository.Name));
            }
        }

        return result;
    }
}