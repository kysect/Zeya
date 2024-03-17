using Kysect.CommonLib.FileSystem;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.SourceCode;

public class SourcesMustNotBeInRootValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly SourcesMustNotBeInRootValidationRule _validationRule;
    private readonly SourcesMustNotBeInRootValidationRule.Arguments _arguments;
    private readonly string _expectedSourceDirectory = "Sources";

    public SourcesMustNotBeInRootValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _validationRule = new SourcesMustNotBeInRootValidationRule(_validationTestFixture.FileSystem);
        _arguments = new SourcesMustNotBeInRootValidationRule.Arguments(_expectedSourceDirectory);
    }

    [Fact]
    public void Validate_EmptySolution_ReturnDiagnosticAboutMissedDirectory()
    {
        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), _arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, _arguments.DiagnosticCode, SourcesMustNotBeInRootValidationRule.Arguments.DirectoryMissedMessage);
    }

    [Fact]
    public void Validate_SolutionInSourceDirectory_ReturnNoDiagnostic()
    {
        string solutionDirectoryPath = _validationTestFixture.FileSystem.Path.Combine(_validationTestFixture.CurrentPath, _expectedSourceDirectory);
        _validationTestFixture.SolutionFileStructureBuilderFactory.Create("Solution")
            .Save(solutionDirectoryPath);

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), _arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(0);
    }

    [Fact]
    public void Validate_SolutionNotInSourceDirectory_ReturnDiagnosticAboutIncorrectPath()
    {
        string solutionDirectoryPath = _validationTestFixture.FileSystem.Path.Combine(_validationTestFixture.CurrentPath, _expectedSourceDirectory);
        string solutionFilePath = _validationTestFixture.FileSystem.Path.Combine(_validationTestFixture.CurrentPath, "Solution.sln");
        DirectoryExtensions.EnsureDirectoryExists(_validationTestFixture.FileSystem, solutionDirectoryPath);
        _validationTestFixture.SolutionFileStructureBuilderFactory.Create("Solution").Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), _arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, _arguments.DiagnosticCode, $"Solution file must not be located in repository root. Founded solution files: {solutionFilePath}");
    }
}