namespace Kysect.Zeya.Abstractions.Contracts;

public interface IGithubRepositoryProvider
{
    IReadOnlyCollection<IClonedRepository> GetGithubOrganizationRepositories(string organization);
    IClonedRepository GetGithubRepository(string owner, string repository);
    IClonedRepository GetLocalRepository(string path);
}