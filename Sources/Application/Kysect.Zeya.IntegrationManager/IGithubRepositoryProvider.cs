using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;

namespace Kysect.Zeya.IntegrationManager;

public interface IGithubRepositoryProvider
{
    IReadOnlyCollection<LocalGithubRepository> GetGithubOrganizationRepositories(string organization, IReadOnlyCollection<string> excludedRepositories);
    LocalGithubRepository GetGithubRepository(string owner, string repository);
    ILocalRepository GetLocalRepository(string path);
}