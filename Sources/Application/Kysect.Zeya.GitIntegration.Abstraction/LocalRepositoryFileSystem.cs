using Kysect.CommonLib.FileSystem;
using System.IO.Abstractions;

namespace Kysect.Zeya.GitIntegration.Abstraction;

public class LocalRepositoryFileSystem(string repositoryRootPath, IFileSystem fileSystem)
{
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

    private string GetFullPathToFile(string partialPath)
    {
        return fileSystem.Path.Combine(GetFullPath(), partialPath);
    }
}