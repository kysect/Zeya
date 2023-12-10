using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IGithubRepositoryAccessor
{
    GithubRepository Repository { get; }

    string GetFullPath();
    bool Exists(string partialPath);
    string ReadFile(string partialPath);

    string GetSolutionFilePath();
    IReadOnlyCollection<string> GetProjectPaths();
}