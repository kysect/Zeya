using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Kysect.Zeya.GitIntegration.Abstraction;

namespace Kysect.Zeya.GithubIntegration.Abstraction.Contracts;

public interface IClonedRepositoryFactory<T> where T : IClonedRepository
{
    T Create(GithubRepositoryName repositoryName);
}