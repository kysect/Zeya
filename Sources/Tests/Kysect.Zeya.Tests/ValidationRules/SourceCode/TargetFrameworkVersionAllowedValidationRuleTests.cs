using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;

namespace Kysect.Zeya.Tests.ValidationRules.SourceCode;

public class TargetFrameworkVersionAllowedValidationRuleTests : ValidationRuleTestBase
{
    private readonly TargetFrameworkVersionAllowedValidationRule _validationRule;

    public TargetFrameworkVersionAllowedValidationRuleTests()
    {
        _validationRule = new TargetFrameworkVersionAllowedValidationRule(RepositorySolutionAccessorFactory);
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

        new SolutionFileStructureBuilder("Solution")
            .AddProject(new ProjectFileStructureBuilder("Project")
                .SetContent(projectFileContent))
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts.ShouldHaveErrorCount(0);

        if (returnDiagnostic)
            DiagnosticCollectorAsserts.ShouldHaveDiagnosticCount(1);
        else
            DiagnosticCollectorAsserts.ShouldHaveDiagnosticCount(0);
    }
}