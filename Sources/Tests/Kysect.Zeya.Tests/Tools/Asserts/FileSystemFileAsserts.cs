using FluentAssertions;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tests.Tools.Asserts;

public class FileSystemFileAsserts
{
    private readonly IFileSystem _fileSystem;
    private readonly string _path;

    public FileSystemFileAsserts(IFileSystem fileSystem, string path)
    {
        _fileSystem = fileSystem;
        _path = path;
    }

    public FileSystemFileAsserts ShouldExists()
    {
        _fileSystem.File.Exists(_path).Should().BeTrue();
        return this;
    }

    public FileSystemFileAsserts ShouldNotExists()
    {
        _fileSystem.File.Exists(_path).Should().BeFalse();
        return this;
    }

    public FileSystemFileAsserts ShouldHaveContent(string content)
    {
        _fileSystem.File.ReadAllText(_path).Should().BeEquivalentTo(content, $"File {_path} should have correct content");
        return this;
    }

    public FileSystemFileAsserts ShouldHaveEmptyContent()
    {
        _fileSystem.File.ReadAllText(_path).Should().BeEmpty();
        return this;
    }
}