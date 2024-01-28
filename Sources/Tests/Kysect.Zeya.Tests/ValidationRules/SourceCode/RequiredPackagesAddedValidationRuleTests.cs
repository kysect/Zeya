using FluentAssertions;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRules.SourceCode;

public class RequiredPackagesAddedValidationRuleTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly RequiredPackagesAddedValidationRule _requiredPackagesAddedValidationRule;
    private readonly string _currentPath;
    private readonly RepositoryDiagnosticCollector _repositoryDiagnosticCollector;
    private readonly ScenarioContext _scenarioContext;
    private readonly XmlDocumentSyntaxFormatter _formatter;

    public RequiredPackagesAddedValidationRuleTests()
    {
        _formatter = new XmlDocumentSyntaxFormatter();
        _fileSystem = new MockFileSystem();
        _currentPath = _fileSystem.Path.GetFullPath(".");

        _requiredPackagesAddedValidationRule = new RequiredPackagesAddedValidationRule(
            new RepositorySolutionAccessorFactory(
                new SolutionFileContentParser(),
                _fileSystem));

        _repositoryDiagnosticCollector = new RepositoryDiagnosticCollector("MockRepository");
        _scenarioContext = RepositoryValidationContextExtensions.CreateScenarioContext(
            RepositoryValidationContext.Create(
                new ClonedRepository(_currentPath, _fileSystem),
                _repositoryDiagnosticCollector));
    }

    [Fact]
    public void Validate_EmptySolution_ReturnDiagnosticAboutMissedDirectoryBuildProps()
    {
        var arguments = new RequiredPackagesAddedValidationRule.Arguments(["RequiredPackage"]);
        new SolutionFileStructureBuilder("Solution")
            .Save(_fileSystem, _currentPath, _formatter);

        _requiredPackagesAddedValidationRule.Execute(_scenarioContext, arguments);
        IReadOnlyCollection<RepositoryValidationDiagnostic> diagnostics = _repositoryDiagnosticCollector.GetDiagnostics();

        diagnostics.Should().HaveCount(1);
        diagnostics.Single().Code.Should().Be(arguments.DiagnosticCode);
        diagnostics.Single().Message.Should().Be(ValidationRuleMessages.DirectoryBuildPropsFileMissed);
    }

    [Fact]
    public void Validate_SolutionWithEmptyDirectoryBuildProps_ReturnDiagnosticAboutMissedPackage()
    {
        var arguments = new RequiredPackagesAddedValidationRule.Arguments(["RequiredPackage"]);
        new SolutionFileStructureBuilder("Solution")
            .AddFile([ValidationConstants.DirectoryBuildPropsFileName], string.Empty)
            .Save(_fileSystem, _currentPath, _formatter);

        _requiredPackagesAddedValidationRule.Execute(_scenarioContext, arguments);
        IReadOnlyCollection<RepositoryValidationDiagnostic> diagnostics = _repositoryDiagnosticCollector.GetDiagnostics();

        // TODO: remove message duplication
        diagnostics.Should().HaveCount(1);
        diagnostics.Single().Code.Should().Be(arguments.DiagnosticCode);
        diagnostics.Single().Message.Should().Be("Package RequiredPackage is not add to Directory.Build.props.");
    }
}