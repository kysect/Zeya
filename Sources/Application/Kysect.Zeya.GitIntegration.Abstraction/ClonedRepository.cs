using Kysect.CommonLib.FileSystem;
using System.IO.Abstractions;

namespace Kysect.Zeya.GitIntegration.Abstraction;

public class ClonedRepository(string repositoryRootPath, IFileSystem fileSystem) : IClonedRepository
{
    public virtual string GetRepositoryName()
    {
        IDirectoryInfo directoryInfo = fileSystem.DirectoryInfo.New(repositoryRootPath);
        return directoryInfo.Name;
    }

    public string GetFullPath()
    {
        return repositoryRootPath;
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
        DirectoryExtensions.EnsureParentDirectoryExists(fileSystem, fullPathToFile);
        fileSystem.File.WriteAllText(fullPathToFile, content);
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