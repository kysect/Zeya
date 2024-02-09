using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;

namespace Kysect.Zeya.GithubIntegration.Abstraction.Contracts;

public interface IClonedRepositoryFactory<T> where T : IClonedRepository
{
    T Create(GithubRepository repository);
}