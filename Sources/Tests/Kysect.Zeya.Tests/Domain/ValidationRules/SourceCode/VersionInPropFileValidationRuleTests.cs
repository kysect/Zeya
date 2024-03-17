using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.RepositoryValidationRules.Rules;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.SourceCode;

public class VersionInPropFileValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly VersionInPropFileValidationRule _validationRule;

    public VersionInPropFileValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _validationRule = new VersionInPropFileValidationRule();
    }

    [Fact]
    public void Execute_NoVersionAttribute_NoDiagnostics()
    {
        var arguments = new VersionInPropFileValidationRule.Arguments();

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(0);
    }

    [Fact]
    public void Execute_VersionAttributeInPropsFile_NoDiagnostics()
    {
        var dotnetProjectFile = DotnetProjectFile.CreateEmpty();
        dotnetProjectFile
            .Properties
            .AddProperty("Version", "1.2.3.4");

        var arguments = new VersionInPropFileValidationRule.Arguments();

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddDirectoryBuildProps(new DirectoryBuildPropsFile(dotnetProjectFile))
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(0);
    }

    [Fact]
    public void Execute_VersionAttributeInCsprojFile_ReturnDiagnostic()
    {
        var dotnetProjectFile = DotnetProjectFile.CreateEmpty();
        dotnetProjectFile
            .Properties
            .AddProperty("Version", "1.2.3.4");

        string projectName = "Project";
        var projectPath = _validationTestFixture.FileSystem.Path.Combine(_validationTestFixture.CurrentPath, projectName, $"{projectName}.csproj");
        var arguments = new VersionInPropFileValidationRule.Arguments();

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName, dotnetProjectFile))
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, RuleDescription.SourceCode.VersionInPropFile, $"Project file must be specified in Directory.Build.props file. Projects with version: {projectPath}");
    }
}