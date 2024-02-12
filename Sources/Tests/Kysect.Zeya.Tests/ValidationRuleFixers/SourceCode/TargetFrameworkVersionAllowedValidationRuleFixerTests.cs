using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.ValidationRules;

namespace Kysect.Zeya.Tests.ValidationRuleFixers.SourceCode;

public class TargetFrameworkVersionAllowedValidationRuleFixerTests : ValidationRuleTestBase
{
    private readonly TargetFrameworkVersionAllowedValidationRuleFixer _fixer;

    public TargetFrameworkVersionAllowedValidationRuleFixerTests()
    {
        _fixer = new TargetFrameworkVersionAllowedValidationRuleFixer(Formatter, Logger);
    }

    [Theory]
    [InlineData("net472", null, null, null, "net472")]

    [InlineData("net8", "net7", null, null, "net7")]
    [InlineData("net8", "net8", null, null, "net8")]
    [InlineData("net8", "net9", null, null, "net9")]
    [InlineData("net8", null, "netstandard2.0", null, "net8")]
    [InlineData("net8", null, null, "net48", "net8")]

    [InlineData("netstandard2.0", "net7", null, null, "netstandard2.0")]
    [InlineData("netstandard2.0", null, "netstandard1.6", null, "netstandard1.6")]
    [InlineData("netstandard2.0", null, "netstandard2.0", null, "netstandard2.0")]
    [InlineData("netstandard2.0", null, "netstandard2.1", null, "netstandard2.1")]
    [InlineData("netstandard2.0", null, null, "net48", "netstandard2.0")]

    [InlineData("net472", "net8", null, null, "net472")]
    [InlineData("net472", null, "netstandard2.0", null, "net472")]
    [InlineData("net472", null, null, "net471", "net471")]
    [InlineData("net472", null, null, "net472", "net472")]
    [InlineData("net472", null, null, "net48", "net48")]
    public void Execute_ReturnExpectedValue(string targetFramework, string? allowedCoreVersion, string? allowedStandardVersion, string? allowedFrameworkVersion, string expectedTargetFramework)
    {
        var arguments = new TargetFrameworkVersionAllowedValidationRule.Arguments(allowedCoreVersion, allowedStandardVersion, allowedFrameworkVersion);
        const string projectName = "SampleProject";
        string expectedProjectContent = $"""
                                         <Project>
                                           <PropertyGroup>
                                             <TargetFramework>{expectedTargetFramework}</TargetFramework>
                                           </PropertyGroup>
                                         </Project>
                                         """;

        var projectFileContent = DotnetProjectFile.CreateEmpty();
        projectFileContent.Properties.SetProperty("TargetFramework", targetFramework);

        new SolutionFileStructureBuilder("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName)
                .SetContent(projectFileContent))
            .Save(FileSystem, CurrentPath, Formatter);

        _fixer.Fix(arguments, Repository);

        FileSystemAsserts
            .File(CurrentPath, projectName, $"{projectName}.csproj")
            .ShouldExists()
            .ShouldHaveContent(expectedProjectContent);
    }
}