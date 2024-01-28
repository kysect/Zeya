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

public class RequiredPackagesAddedValidationRuleTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly RequiredPackagesAddedValidationRule _requiredPackagesAddedValidationRule;
    private readonly string _currentPath;
    private readonly ScenarioContext _scenarioContext;
    private readonly XmlDocumentSyntaxFormatter _formatter;
    private readonly RepositoryDiagnosticCollectorAsserts _diagnosticCollectorAsserts;

    public RequiredPackagesAddedValidationRuleTests()
    {
        _formatter = new XmlDocumentSyntaxFormatter();
        _fileSystem = new MockFileSystem();
        _currentPath = _fileSystem.Path.GetFullPath(".");

        _requiredPackagesAddedValidationRule = new RequiredPackagesAddedValidationRule(
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
        var arguments = new RequiredPackagesAddedValidationRule.Arguments(["RequiredPackage"]);
        new SolutionFileStructureBuilder("Solution")
            .Save(_fileSystem, _currentPath, _formatter);

        _requiredPackagesAddedValidationRule.Execute(_scenarioContext, arguments);

        _diagnosticCollectorAsserts
            .ShouldHaveCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ValidationRuleMessages.DirectoryBuildPropsFileMissed);
    }

    [Fact]
    public void Validate_SolutionWithEmptyDirectoryBuildProps_ReturnDiagnosticAboutMissedPackage()
    {
        var arguments = new RequiredPackagesAddedValidationRule.Arguments(["RequiredPackage"]);
        new SolutionFileStructureBuilder("Solution")
            .AddFile([ValidationConstants.DirectoryBuildPropsFileName], string.Empty)
            .Save(_fileSystem, _currentPath, _formatter);

        _requiredPackagesAddedValidationRule.Execute(_scenarioContext, arguments);

        _diagnosticCollectorAsserts
            .ShouldHaveCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Package RequiredPackage is not add to Directory.Build.props.");
    }
}