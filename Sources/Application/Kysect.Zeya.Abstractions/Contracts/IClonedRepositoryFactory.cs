using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IClonedRepositoryFactory<T> where T : IClonedRepository
{
    T Create(GithubRepository repository);
}