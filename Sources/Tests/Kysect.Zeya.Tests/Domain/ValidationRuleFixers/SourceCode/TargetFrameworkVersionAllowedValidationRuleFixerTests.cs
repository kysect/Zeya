using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.SourceCode;

public class TargetFrameworkVersionAllowedValidationRuleFixerTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly TargetFrameworkVersionAllowedValidationRuleFixer _fixer;

    public TargetFrameworkVersionAllowedValidationRuleFixerTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _fixer = new TargetFrameworkVersionAllowedValidationRuleFixer(_validationTestFixture.Formatter, _validationTestFixture.GetLogger<TargetFrameworkVersionAllowedValidationRuleFixer>());
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

        _validationTestFixture.SolutionFileStructureBuilderFactory.Create("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName)
                .SetContent(projectFileContent))
            .Save(_validationTestFixture.CurrentPath);

        LocalGithubRepository localGithubRepository = _validationTestFixture.CreateGithubRepository();
        _fixer.Fix(arguments, localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, projectName, $"{projectName}.csproj")
            .ShouldExists()
            .ShouldHaveContent(expectedProjectContent);
    }
}