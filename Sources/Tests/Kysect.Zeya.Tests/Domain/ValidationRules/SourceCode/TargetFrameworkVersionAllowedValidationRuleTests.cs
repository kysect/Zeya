using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.SourceCode;

public class TargetFrameworkVersionAllowedValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly TargetFrameworkVersionAllowedValidationRule _validationRule;

    public TargetFrameworkVersionAllowedValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _validationRule = new TargetFrameworkVersionAllowedValidationRule();
    }

    [Theory]
    [InlineData("net472", null, null, "net472", false)]
    [InlineData("net472", null, null, "net48", true)]
    [InlineData("net9", "net8", null, null, true)]
    [InlineData("net8", "net8", null, null, false)]
    [InlineData("net7", "net8", null, null, true)]
    [InlineData("netstandard2.0", null, "netstandard2.0", null, false)]
    [InlineData("netstandard2.1", null, "netstandard2.0", null, true)]
    [InlineData("net472", "net8", null, null, true)]
    public void Execute_ReturnExpectedValue(string projectTarget, string? allowedCoreVersion, string? allowedStandardVersion, string? allowedFrameworkVersion, bool returnDiagnostic)
    {
        var arguments = new TargetFrameworkVersionAllowedValidationRule.Arguments(allowedCoreVersion, allowedStandardVersion, allowedFrameworkVersion);

        var projectFileContent = DotnetProjectFile.CreateEmpty();
        projectFileContent.Properties.SetProperty("TargetFramework", projectTarget);

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddProject(new ProjectFileStructureBuilder("Project")
                .SetContent(projectFileContent))
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts.ShouldHaveErrorCount(0);

        if (returnDiagnostic)
            _validationTestFixture.DiagnosticCollectorAsserts.ShouldHaveDiagnosticCount(1);
        else
            _validationTestFixture.DiagnosticCollectorAsserts.ShouldHaveDiagnosticCount(0);
    }
}