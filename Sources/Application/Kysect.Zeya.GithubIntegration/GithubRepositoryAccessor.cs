using Kysect.CommonLib.BaseTypes.Extensions;
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
        var fileInfo = fileSystem.FileInfo.New(fullPathToFile);
        EnsureContainingDirectoryExists(fileSystem, fileInfo);
        fileSystem.File.WriteAllText(fullPathToFile, content);
    }

    // TODO: move to common lib
    private static void EnsureContainingDirectoryExists(IFileSystem fileSystem, IFileInfo fileInfo)
    {
        fileSystem.ThrowIfNull();
        fileInfo.ThrowIfNull();

        if (fileInfo.Directory == null)
            throw new ArgumentException($"Cannot get directory for path {fileInfo.FullName}");

        if (!fileSystem.Directory.Exists(fileInfo.Directory.FullName))
            fileSystem.Directory.CreateDirectory(fileInfo.Directory.FullName);
    }

    public string GetWorkflowPath(string workflowName)
    {
        return fileSystem.Path.Combine(".github", "workflows", workflowName);
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