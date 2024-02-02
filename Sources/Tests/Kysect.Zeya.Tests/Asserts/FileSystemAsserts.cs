using System.IO.Abstractions;

namespace Kysect.Zeya.Tests.Asserts;

public class FileSystemAsserts
{
    private readonly IFileSystem _fileSystem;

    public FileSystemAsserts(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public FileSystemFileAsserts File(params string[] pathParts)
    {
        return new FileSystemFileAsserts(_fileSystem, _fileSystem.Path.Combine(pathParts));
    }
}