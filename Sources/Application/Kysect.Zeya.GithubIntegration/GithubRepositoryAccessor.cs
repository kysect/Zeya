using System.IO.Abstractions;
using Kysect.GithubUtils.RepositorySync;
using Kysect.Zeya.Abstractions;
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

    public string GetSolutionFilePath()
    {
        var repositoryPath = GetFullPath();

        var solutions = fileSystem.Directory.EnumerateFiles(repositoryPath, "*.sln", SearchOption.AllDirectories).ToList();
        if (solutions.Count == 0)
            throw new ZeyaException($"Repository {repositoryPath} does not contains .sln files");

        // TODO: investigate this path
        if (solutions.Count > 1)
            throw new ZeyaException($"Repository {repositoryPath} has more than one solution file.");

        return solutions.Single();
    }

    // TODO: this is not correct. Need to get path from solution
    public IReadOnlyCollection<string> GetProjectPaths()
    {
        return fileSystem
            .Directory
            .EnumerateFiles(GetFullPath(), "*.csproj", SearchOption.AllDirectories)
            .ToList();
    }
    
    private string GetFullPathToFile(string partialPath)
    {
        return fileSystem.Path.Combine(GetFullPath(), partialPath);
    }
}