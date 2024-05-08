using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.FileSystem;
using System.IO.Abstractions;

namespace Kysect.Zeya.LocalRepositoryAccess;

public class LocalRepositoryFileSystem(string repositoryRootPath, IFileSystem fileSystem)
{
    public string GetFullPath()
    {
        return repositoryRootPath;
    }

    public bool Exists(string path)
    {
        path.ThrowIfNull();

        return fileSystem.File.Exists(GetFullPathToFile(path));
    }

    public string ReadAllText(string path)
    {
        path.ThrowIfNull();

        return fileSystem.File.ReadAllText(GetFullPathToFile(path));
    }

    public void WriteAllText(string path, string content)
    {
        path.ThrowIfNull();
        content.ThrowIfNull();

        string fullPathToFile = GetFullPathToFile(path);
        fileSystem.EnsureParentDirectoryExists(fullPathToFile);
        fileSystem.File.WriteAllText(fullPathToFile, content);
    }

    private string GetFullPathToFile(string path)
    {
        string fullPath = GetFullPath();
        if (path.StartsWith(fullPath))
            return path;
        return fileSystem.Path.Combine(fullPath, path);
    }
}