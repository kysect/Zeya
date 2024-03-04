using FluentAssertions;
using Kysect.Zeya.LocalRepositoryAccess;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.LocalRepositoryAccess;

public class LocalRepositoryTests
{
    private readonly MockFileSystem _fileSystem;

    public LocalRepositoryTests()
    {
        _fileSystem = new MockFileSystem();
    }

    [Fact]
    public void GetRepositoryName_ReturnDirectoryName()
    {
        string repositoryRootPath = _fileSystem.Path.Combine("Path", "To", "SomeDirectory");
        var localRepository = new LocalRepository(repositoryRootPath, _fileSystem);

        string name = localRepository.GetRepositoryName();

        name.Should().Be("SomeDirectory");
    }
}