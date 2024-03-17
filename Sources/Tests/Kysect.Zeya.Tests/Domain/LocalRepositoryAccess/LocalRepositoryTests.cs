using FluentAssertions;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
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
        var dotnetSolutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, new SolutionFileContentParser(), new XmlDocumentSyntaxFormatter());
        var localRepository = new LocalRepository(repositoryRootPath, LocalRepositorySolutionManager.DefaultMask, _fileSystem, dotnetSolutionModifierFactory);

        string name = localRepository.GetRepositoryName();

        name.Should().Be("SomeDirectory");
    }
}