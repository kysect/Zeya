using FluentAssertions;
using Kysect.DotnetSlnGenerator.Builders;
using Kysect.DotnetSlnGenerator.Models;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRules.SourceCode;

[TestFixture]
public class ArtifactsOutputEnabledValidationRuleTests
{
    private ArtifactsOutputEnabledValidationRule _validationRule;

    private MockFileSystem _fileSystem;
    private RepositoryDiagnosticCollector _repositoryDiagnosticCollector;
    private ScenarioContext _scenarioContext;

    private string _currentPath;
    private string _repositoryName;

    [SetUp]
    public void Setup()
    {
        _fileSystem = new MockFileSystem();
        _currentPath = _fileSystem.Path.GetFullPath(".");

        _validationRule = new ArtifactsOutputEnabledValidationRule(
            new RepositorySolutionAccessorFactory(
                new SolutionFileParser(TestLoggerProvider.GetLogger()),
                _fileSystem));

        _repositoryName = "MockRepository";
        _repositoryDiagnosticCollector = new RepositoryDiagnosticCollector(_repositoryName);
        _scenarioContext = RepositoryValidationContextExtensions.CreateScenarioContext(
            RepositoryValidationContext.Create(
                new ClonedRepository(_currentPath, _fileSystem),
                _repositoryDiagnosticCollector));

    }

    [Test]
    public void Validate_EmptySolution_ReturnDiagnosticAboutMissedDirectoryBuildProps()
    {
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();
        var diagnostic = new RepositoryValidationDiagnostic(arguments.DiagnosticCode, _repositoryName, ValidationRuleMessages.DirectoryBuildPropsFileMissed, ArtifactsOutputEnabledValidationRule.Arguments.DefaultSeverity);
        new DotnetSolutionBuilder("Solution")
            //.AddFile(new SolutionFileInfo([ValidationConstants.DirectoryBuildPropsFileName], string.Empty))
            .Save(_fileSystem, _currentPath);

        _validationRule.Execute(_scenarioContext, arguments);
        IReadOnlyCollection<RepositoryValidationDiagnostic> diagnostics = _repositoryDiagnosticCollector.GetDiagnostics();

        diagnostics.Should().BeEquivalentTo(new[] { diagnostic });
    }

    [Test]
    public void Validate_EmptyDirectoryBuildProps_ReturnDiagnosticAboutMissedUseArtifactsOutput()
    {
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();
        var diagnostic = new RepositoryValidationDiagnostic(
            arguments.DiagnosticCode,
            _repositoryName,
            ArtifactsOutputEnabledValidationRule.Arguments.UseArtifactsOutputOptionMissed,
            ArtifactsOutputEnabledValidationRule.Arguments.DefaultSeverity);

        new DotnetSolutionBuilder("Solution")
            .AddFile(new SolutionFileInfo([ValidationConstants.DirectoryBuildPropsFileName], string.Empty))
            .Save(_fileSystem, _currentPath);

        _validationRule.Execute(_scenarioContext, arguments);
        IReadOnlyCollection<RepositoryValidationDiagnostic> diagnostics = _repositoryDiagnosticCollector.GetDiagnostics();

        diagnostics.Should().BeEquivalentTo(new[] { diagnostic });
    }

    [Test]
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
        var diagnostic = new RepositoryValidationDiagnostic(
            arguments.DiagnosticCode,
            _repositoryName,
            ArtifactsOutputEnabledValidationRule.Arguments.UseArtifactsOutputOptionMustBeTrue,
            ArtifactsOutputEnabledValidationRule.Arguments.DefaultSeverity);

        new DotnetSolutionBuilder("Solution")
            .AddFile(new SolutionFileInfo([ValidationConstants.DirectoryBuildPropsFileName], directoryBuildPropsContent))
            .Save(_fileSystem, _currentPath);

        _validationRule.Execute(_scenarioContext, arguments);
        IReadOnlyCollection<RepositoryValidationDiagnostic> diagnostics = _repositoryDiagnosticCollector.GetDiagnostics();

        diagnostics.Should().BeEquivalentTo(new[] { diagnostic });
    }

    [Test]
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
        var diagnostic = new RepositoryValidationDiagnostic(
            arguments.DiagnosticCode,
            _repositoryName,
            ArtifactsOutputEnabledValidationRule.Arguments.UseArtifactsOutputOptionMustBeTrue,
            ArtifactsOutputEnabledValidationRule.Arguments.DefaultSeverity);

        new DotnetSolutionBuilder("Solution")
            .AddFile(new SolutionFileInfo([ValidationConstants.DirectoryBuildPropsFileName], directoryBuildPropsContent))
            .Save(_fileSystem, _currentPath);

        _validationRule.Execute(_scenarioContext, arguments);
        IReadOnlyCollection<RepositoryValidationDiagnostic> diagnostics = _repositoryDiagnosticCollector.GetDiagnostics();

        diagnostics.Should().BeEmpty();
    }
}