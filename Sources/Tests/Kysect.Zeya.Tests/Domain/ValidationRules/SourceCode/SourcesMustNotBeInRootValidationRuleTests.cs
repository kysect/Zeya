using Kysect.CommonLib.FileSystem;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.SourceCode;

public class SourcesMustNotBeInRootValidationRuleTests : ValidationRuleTestBase
{
    private readonly SourcesMustNotBeInRootValidationRule _validationRule;
    private readonly SourcesMustNotBeInRootValidationRule.Arguments _arguments;
    private readonly string _expectedSourceDirectory = "Sources";

    public SourcesMustNotBeInRootValidationRuleTests()
    {
        _validationRule = new SourcesMustNotBeInRootValidationRule(FileSystem);
        _arguments = new SourcesMustNotBeInRootValidationRule.Arguments(_expectedSourceDirectory);
    }

    [Fact]
    public void Validate_EmptySolution_ReturnDiagnosticAboutMissedDirectory()
    {
        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, _arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, _arguments.DiagnosticCode, SourcesMustNotBeInRootValidationRule.Arguments.DirectoryMissedMessage);
    }

    [Fact]
    public void Validate_SolutionInSourceDirectory_ReturnNoDiagnostic()
    {
        string solutionDirectoryPath = FileSystem.Path.Combine(CurrentPath, _expectedSourceDirectory);
        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, solutionDirectoryPath, Formatter);

        _validationRule.Execute(Context, _arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(0);
    }

    [Fact]
    public void Validate_SolutionNotInSourceDirectory_ReturnDiagnosticAboutIncorrectPath()
    {
        string solutionDirectoryPath = FileSystem.Path.Combine(CurrentPath, _expectedSourceDirectory);
        string solutionFilePath = FileSystem.Path.Combine(CurrentPath, "Solution.sln");
        DirectoryExtensions.EnsureDirectoryExists(FileSystem, solutionDirectoryPath);
        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, _arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, _arguments.DiagnosticCode, $"Sources must be located in {solutionDirectoryPath}. Founded solution files: {solutionFilePath}");
    }
}