using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IGithubRepositoryProvider
{
    IReadOnlyCollection<GithubRepository> GetAll();
}