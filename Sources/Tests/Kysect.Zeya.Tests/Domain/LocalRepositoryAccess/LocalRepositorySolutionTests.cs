using FluentAssertions;
using Kysect.Zeya.LocalRepositoryAccess;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.LocalRepositoryAccess;

public class LocalRepositorySolutionTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly string _repositoryDirectoryPath;
    private readonly string _solutionPath;

    public LocalRepositorySolutionTests()
    {
        _fileSystem = new MockFileSystem();
        _repositoryDirectoryPath = _fileSystem.Path.Combine("Path", "To", "Directory");
        _solutionPath = _fileSystem.Path.Combine(_repositoryDirectoryPath, "Sources", "Solution.sln");
    }

    [Fact]
    public void GetSolutionDirectoryPath_ReturnCorrectPath()
    {
        string expected = _fileSystem.Path.Combine(_repositoryDirectoryPath, "Sources");
        string fullPath = _fileSystem.Path.GetFullPath(expected);

        new LocalRepositorySolution(_repositoryDirectoryPath, _solutionPath, _fileSystem)
            .GetSolutionDirectoryPath()
            .Should()
            .Be(fullPath);
    }

    [Fact]
    public void GetDirectoryPackagePropsPath_ReturnCorrectPath()
    {
        new LocalRepositorySolution(_repositoryDirectoryPath, _solutionPath, _fileSystem)
            .GetDirectoryPackagePropsPath()
            .Should()
            .Be("Sources\\Directory.Packages.props");
    }

    [Fact]
    public void GetDirectoryBuildPropsPath_ReturnCorrectPath()
    {
        new LocalRepositorySolution(_repositoryDirectoryPath, _solutionPath, _fileSystem)
            .GetDirectoryBuildPropsPath()
            .Should()
            .Be("Sources\\Directory.Build.props");
    }
}