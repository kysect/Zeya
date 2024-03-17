using FluentAssertions;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.LocalRepositoryAccess;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.LocalRepositoryAccess;

public class LocalRepositorySolutionTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly string _repositoryDirectoryPath;
    private readonly string _solutionPath;
    private readonly DotnetSolutionModifierFactory _solutionModifierFactory;

    public LocalRepositorySolutionTests()
    {
        _fileSystem = new MockFileSystem();
        _repositoryDirectoryPath = _fileSystem.Path.Combine("Path", "To", "Directory");
        _solutionPath = _fileSystem.Path.Combine(_repositoryDirectoryPath, "Sources", "Solution.sln");
        _solutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, new SolutionFileContentParser(), new XmlDocumentSyntaxFormatter());
    }

    [Fact]
    public void GetSolutionDirectoryPath_ReturnCorrectPath()
    {
        string expected = _fileSystem.Path.Combine(_repositoryDirectoryPath, "Sources");
        string fullPath = _fileSystem.Path.GetFullPath(expected);

        new LocalRepositorySolution(_repositoryDirectoryPath, _solutionPath, _fileSystem, _solutionModifierFactory)
            .GetSolutionDirectoryPath()
            .Should()
            .Be(fullPath);
    }

    [Fact]
    public void GetDirectoryPackagePropsPath_ReturnCorrectPath()
    {
        string expected = _fileSystem.Path.Combine("Sources", SolutionItemNameConstants.DirectoryPackagesProps);

        new LocalRepositorySolution(_repositoryDirectoryPath, _solutionPath, _fileSystem, _solutionModifierFactory)
            .GetDirectoryPackagePropsPath()
            .Should()
            .Be(expected);
    }

    [Fact]
    public void GetDirectoryBuildPropsPath_ReturnCorrectPath()
    {
        string expected = _fileSystem.Path.Combine("Sources", SolutionItemNameConstants.DirectoryBuildProps);

        new LocalRepositorySolution(_repositoryDirectoryPath, _solutionPath, _fileSystem, _solutionModifierFactory)
            .GetDirectoryBuildPropsPath()
            .Should()
            .Be(expected);
    }
}