using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IGithubRepositoryAccessor
{
    GithubRepository Repository { get; }
    string GetFullPath();
}