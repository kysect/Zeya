using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.Tests.Asserts;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRules.SourceCode;

public class ArtifactsOutputEnabledValidationRuleTests
{
    private readonly ArtifactsOutputEnabledValidationRule _validationRule;

    private readonly MockFileSystem _fileSystem;
    private readonly ScenarioContext _scenarioContext;
    private readonly XmlDocumentSyntaxFormatter _formatter;
    private readonly RepositoryDiagnosticCollectorAsserts _diagnosticCollectorAsserts;

    private readonly string _currentPath;

    public ArtifactsOutputEnabledValidationRuleTests()
    {
        _formatter = new XmlDocumentSyntaxFormatter();
        _fileSystem = new MockFileSystem();
        _currentPath = _fileSystem.Path.GetFullPath(".");

        _validationRule = new ArtifactsOutputEnabledValidationRule(
            new RepositorySolutionAccessorFactory(
                new SolutionFileContentParser(),
                _fileSystem));

        _diagnosticCollectorAsserts = new RepositoryDiagnosticCollectorAsserts("MockRepository");
        _scenarioContext = RepositoryValidationContextExtensions.CreateScenarioContext(
            RepositoryValidationContext.Create(
                new ClonedRepository(_currentPath, _fileSystem),
                _diagnosticCollectorAsserts.GetCollector()));

    }

    [Fact]
    public void Validate_EmptySolution_ReturnDiagnosticAboutMissedDirectoryBuildProps()
    {
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();

        new SolutionFileStructureBuilder("Solution")
            .Save(_fileSystem, _currentPath, _formatter);

        _validationRule.Execute(_scenarioContext, arguments);

        _diagnosticCollectorAsserts
            .ShouldHaveCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ValidationRuleMessages.DirectoryBuildPropsFileMissed);
    }

    [Fact]
    public void Validate_EmptyDirectoryBuildProps_ReturnDiagnosticAboutMissedUseArtifactsOutput()
    {
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();

        new SolutionFileStructureBuilder("Solution")
            .AddFile([ValidationConstants.DirectoryBuildPropsFileName], string.Empty)
            .Save(_fileSystem, _currentPath, _formatter);

        _validationRule.Execute(_scenarioContext, arguments);

        _diagnosticCollectorAsserts
            .ShouldHaveCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ArtifactsOutputEnabledValidationRule.Arguments.UseArtifactsOutputOptionMustBeTrue);
    }

    [Fact]
    public void Validate_UseArtifactsOutputSetToFalse_ReturnDiagnosticAboutMissedUseArtifactsOutput()
    {
        var directoryBuildPropsContent = """
                                         <Project>
                                             <PropertyGroup>
                                                 <UseArtifactsOutput>false</UseArtifactsOutput>
                                             </PropertyGroup>
                                         </Project>
                                         """;
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();

        new SolutionFileStructureBuilder("Solution")
            .AddFile([ValidationConstants.DirectoryBuildPropsFileName], directoryBuildPropsContent)
            .Save(_fileSystem, _currentPath, _formatter);

        _validationRule.Execute(_scenarioContext, arguments);

        _diagnosticCollectorAsserts
            .ShouldHaveCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ArtifactsOutputEnabledValidationRule.Arguments.UseArtifactsOutputOptionMustBeTrue);
    }

    [Fact]
    public void Validate_UseArtifactsOutputSetToTrue_NoDiagnostic()
    {
        var directoryBuildPropsContent = """
                                         <Project>
                                             <PropertyGroup>
                                                 <UseArtifactsOutput>true</UseArtifactsOutput>
                                             </PropertyGroup>
                                         </Project>
                                         """;
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();

        new SolutionFileStructureBuilder("Solution")
            .AddFile([ValidationConstants.DirectoryBuildPropsFileName], directoryBuildPropsContent)
            .Save(_fileSystem, _currentPath, _formatter);

        _validationRule.Execute(_scenarioContext, arguments);

        _diagnosticCollectorAsserts
            .ShouldHaveCount(0);
    }
}