using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IClonedRepositoryFactory
{
    IClonedRepository Create(GithubRepository repository);
}