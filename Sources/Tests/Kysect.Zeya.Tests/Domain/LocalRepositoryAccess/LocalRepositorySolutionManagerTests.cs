using FluentAssertions;
using Kysect.CommonLib.FileSystem;
using Kysect.Zeya.Common;
using Kysect.Zeya.LocalRepositoryAccess;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.LocalRepositoryAccess;

public class LocalRepositorySolutionManagerTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly string _repositoryRootPath;
    private readonly LocalRepositorySolutionManager _localRepositorySolutionManager;

    public LocalRepositorySolutionManagerTests()
    {
        _fileSystem = new MockFileSystem();
        _repositoryRootPath = _fileSystem.FileSystem.Path.GetFullPath(_fileSystem.Path.Combine(".", "Repository"));
        DirectoryExtensions.EnsureDirectoryExists(_fileSystem, _repositoryRootPath);
        _localRepositorySolutionManager = new LocalRepositorySolutionManager(_repositoryRootPath, LocalRepositorySolutionManager.DefaultMask, _fileSystem);
    }

    [Fact]
    public void GetSolution_NoSolution_ReturnExceptionAboutNoSolution()
    {
        var exception = Assert.Throws<ZeyaException>(() =>
        {
            _localRepositorySolutionManager.GetSolution();
        });

        exception.Message.Should().Be($"Repository {_repositoryRootPath} does not contains .sln files");
    }

    [Fact]
    public void GetSolution_OneSolution_ReturnSolution()
    {
        var slnFilePath = _fileSystem.Path.Combine(_repositoryRootPath, "First.sln");
        _fileSystem.AddFile(slnFilePath, new MockFileData(string.Empty));

        LocalRepositorySolution localRepositorySolution = _localRepositorySolutionManager.GetSolution();

        localRepositorySolution.GetSolutionDirectoryPath().Should().Be(_repositoryRootPath);
    }

    [Fact]
    public void GetSolution_TwoSolution_ThrowExceptionAboutNotSupportedMoreThatOneSolution()
    {
        string firstSln = _fileSystem.Path.Combine(_repositoryRootPath, "First.sln");
        string secondSln = _fileSystem.Path.Combine(_repositoryRootPath, "Second.sln");

        _fileSystem.AddFile(firstSln, new MockFileData(string.Empty));
        _fileSystem.AddFile(secondSln, new MockFileData(string.Empty));

        var zeyaException = Assert.Throws<ZeyaException>(() =>
        {
            _localRepositorySolutionManager.GetSolution();
        });

        zeyaException.Message.Should().Be($"Repository {_repositoryRootPath} has more than one solution file.");
    }

    [Fact]
    public void GetSolution_TwoSolutionWithMask_ReturnSolutionMatchedByMask()
    {
        string firstSln = _fileSystem.Path.Combine(_repositoryRootPath, "First.sln");
        string secondSln = _fileSystem.Path.Combine(_repositoryRootPath, "Second.sln");

        _fileSystem.AddFile(firstSln, new MockFileData(string.Empty));
        _fileSystem.AddFile(secondSln, new MockFileData(string.Empty));

        var localRepositorySolutionManager = new LocalRepositorySolutionManager(_repositoryRootPath, "Second.sln", _fileSystem);
        LocalRepositorySolution localRepositorySolution = localRepositorySolutionManager.GetSolution();

        localRepositorySolution.GetSolutionDirectoryPath().Should().Be(_repositoryRootPath);
    }
}