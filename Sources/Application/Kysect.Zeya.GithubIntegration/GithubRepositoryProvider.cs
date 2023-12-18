using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.OrganizationsSync.RepositoryDiscovering;
using Kysect.Zeya.Abstractions.Contracts;
using Microsoft.Extensions.Options;
using Octokit;
using GithubRepository = Kysect.Zeya.Abstractions.Models.GithubRepository;

namespace Kysect.Zeya.GithubIntegration;

public class GithubRepositoryProvider : IGithubRepositoryProvider
{
    private readonly IGitHubClient _gitHub;
    private readonly GithubIntegrationOptions _githubIntegrationOptions;

    public GithubRepositoryProvider(IGitHubClient gitHub, IOptions<GithubIntegrationOptions> githubIntegrationOptions)
    {
        _gitHub = gitHub;
        githubIntegrationOptions.ThrowIfNull();

        _githubIntegrationOptions = githubIntegrationOptions.Value;
    }

    public IReadOnlyCollection<GithubRepository> GetAll()
    {
        return GetAllInner().Result;
    }

    private async Task<IReadOnlyCollection<GithubRepository>> GetAllInner()
    {
        var result = new List<GithubRepository>();

        HashSet<string> skipList = _githubIntegrationOptions.ExcludedRepositories.ToHashSet();
        var gitHubRepositoryDiscoveryService = new GitHubRepositoryDiscoveryService(_gitHub);
        foreach (GithubRepositoryBranch repository in await gitHubRepositoryDiscoveryService.GetRepositories(_githubIntegrationOptions.IncludedOrganization))
        {
            if (!skipList.Contains(repository.Name))
            {
                result.Add(new GithubRepository(_githubIntegrationOptions.IncludedOrganization, repository.Name));
            }
        }

        return result;
    }
}