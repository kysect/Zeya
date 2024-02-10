using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;

namespace Kysect.Zeya.IntegrationManager;

public interface IGithubRepositoryProvider
{
    IReadOnlyCollection<ClonedGithubRepository> GetGithubOrganizationRepositories(string organization);
    ClonedGithubRepository GetGithubRepository(string owner, string repository);
    IClonedRepository GetLocalRepository(string path);
}