using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IGithubRepositoryStorage
{
    IReadOnlyCollection<GithubRepository> GetAll();
    void Add(GithubRepository githubRepository);
    void Remove(GithubRepository githubRepository);
}