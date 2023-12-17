using Kysect.GithubUtils.RepositorySync;
using Kysect.Zeya.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;

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

    public string ReadAllText(string partialPath)
    {
        return fileSystem.File.ReadAllText(GetFullPathToFile(partialPath));
    }

    public void WriteAllText(string partialPath, string content)
    {
        string fullPathToFile = GetFullPathToFile(partialPath);
        fileSystem.File.WriteAllText(fullPathToFile, content);
    }

    public string GetWorkflowPath(string workflowName)
    {
        return fileSystem.Path.Combine(".github", "workflow", workflowName);
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

    private string GetFullPathToFile(string partialPath)
    {
        return fileSystem.Path.Combine(GetFullPath(), partialPath);
    }
}