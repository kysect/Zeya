using System.IO.Abstractions;
using Kysect.GithubUtils.RepositorySync;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.GithubIntegration;

public class GithubRepositoryAccessor(GithubRepository repository, IPathFormatStrategy pathFormatStrategy, IFileSystem fileSystem)
    : IGithubRepositoryAccessor
{
    public GithubRepository Repository { get; } = repository;

    public string GetFullPath()
    {
        return pathFormatStrategy.GetPathToRepository(Repository.Owner, Repository.Name);
    }

    public bool Exists(string partialPath)
    {
        return fileSystem.File.Exists(GetFullPathToFile(partialPath));
    }

    public string ReadFile(string partialPath)
    {
        return fileSystem.File.ReadAllText(GetFullPathToFile(partialPath));
    }

    private string GetFullPathToFile(string partialPath)
    {
        return Path.Combine(GetFullPath(), partialPath);
    }
}