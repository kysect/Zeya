using Kysect.Zeya.GitIntegration.Abstraction;

namespace Kysect.Zeya.GithubIntegration.Abstraction.Contracts;

public interface IGithubRepositoryProvider
{
    IReadOnlyCollection<IClonedRepository> GetGithubOrganizationRepositories(string organization);
    IClonedRepository GetGithubRepository(string owner, string repository);
    IClonedRepository GetLocalRepository(string path);
}