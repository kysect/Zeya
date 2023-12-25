using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetSlnGenerator;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRules.SourceCode;

public class RequiredPackagesAddedValidationRuleTests
{
    private MockFileSystem _fileSystem;
    private RequiredPackagesAddedValidationRule _requiredPackagesAddedValidationRule;
    private string _currentPath;
    private RepositoryDiagnosticCollector _repositoryDiagnosticCollector;
    private ScenarioContext _scenarioContext;

    [SetUp]
    public void Setup()
    {
        _fileSystem = new MockFileSystem();
        _currentPath = _fileSystem.Path.GetFullPath(".");

        _requiredPackagesAddedValidationRule = new RequiredPackagesAddedValidationRule(
            new RepositorySolutionAccessorFactory(
                new SolutionFileParser(DefaultLoggerConfiguration.CreateConsoleLogger()),
                _fileSystem));

        _repositoryDiagnosticCollector = new RepositoryDiagnosticCollector("MockRepository");
        _scenarioContext = RepositoryValidationContextExtensions.CreateScenarioContext(
            RepositoryValidationContext.Create(
                new ClonedRepository(_currentPath, _fileSystem),
                _repositoryDiagnosticCollector));
    }

    [Test]
    public void Validate_EmptySolution_ReturnDiagnosticAboutMissedDirectoryBuildProps()
    {
        var arguments = new RequiredPackagesAddedValidationRule.Arguments(["RequiredPackage"]);
        new DotnetSolutionBuilder("Solution")
            .Save(_fileSystem, _currentPath);

        _requiredPackagesAddedValidationRule.Execute(_scenarioContext, arguments);
        IReadOnlyCollection<RepositoryValidationDiagnostic> diagnostics = _repositoryDiagnosticCollector.GetDiagnostics();

        // TODO: remove message duplication
        diagnostics.Should().HaveCount(1);
        diagnostics.Single().Code.Should().Be(arguments.DiagnosticCode);
        diagnostics.Single().Message.Should().Be("Directory.Build.props file is not exists.");
    }
}