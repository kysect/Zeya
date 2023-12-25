using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;

namespace Kysect.Zeya.GithubIntegration;

public class GithubRepositoryAccessor(GithubRepository repository, ILocalStoragePathFactory pathFormatStrategy, IFileSystem fileSystem)
    : IGithubRepositoryAccessor
{
    public GithubRepository Repository { get; } = repository;

    public string GetFullPath()
    {
        return pathFormatStrategy.GetPathToRepository(new GithubUtils.Models.GithubRepository(Repository.Owner, Repository.Name));
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

    private string GetFullPathToFile(string partialPath)
    {
        return fileSystem.Path.Combine(GetFullPath(), partialPath);
    }
}