using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;

namespace Kysect.Zeya.Application;

public interface IGithubRepositoryProvider
{
    IReadOnlyCollection<LocalGithubRepository> GetGithubOrganizationRepositories(string organization, IReadOnlyCollection<string> excludedRepositories);
    LocalGithubRepository GetGithubRepository(string owner, string repository);
    ILocalRepository GetLocalRepository(string path);
}