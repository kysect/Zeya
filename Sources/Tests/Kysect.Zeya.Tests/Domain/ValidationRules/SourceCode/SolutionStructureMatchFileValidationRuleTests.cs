using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.Zeya.RepositoryValidationRules.Rules;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.SourceCode;

public class SolutionStructureMatchFileValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly SolutionStructureMatchFileValidationRule _validationRule;

    public SolutionStructureMatchFileValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _validationRule = _validationTestFixture.GetRequiredService<SolutionStructureMatchFileValidationRule>();
    }

    [Fact]
    public void Execute_ForCorrectSolution_NoDiagnostics()
    {
        var arguments = new SolutionStructureMatchFileValidationRule.Arguments();

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddProject(new ProjectFileStructureBuilder("FirstProject"))
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }

    [Fact]
    public void Execute_ProjectNameAndPathDoesNotMatch_ReturnDiagnostic()
    {
        var arguments = new SolutionStructureMatchFileValidationRule.Arguments();

        string expectedProjectPath = _validationTestFixture.FileSystem.Path.Combine("Project1", "Project1.csproj");
        string projectPath = _validationTestFixture.FileSystem.Path.Combine("OldName", "Project1.csproj");
        string solutionFileContent = new SolutionFileStringBuilder()
            .AddProject("Project1", projectPath)
            .Build();

        _validationTestFixture.FileSystem.AddFile("Solution.sln", new MockFileData(solutionFileContent));
        _validationTestFixture.FileSystem.AddFile(projectPath, new MockFileData(string.Empty));

        _validationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, RuleDescription.SourceCode.SolutionStructureMatchFile, $"Project name does not match with directory name. Project file system path: {projectPath}, solution structure path: {expectedProjectPath}");
    }
}