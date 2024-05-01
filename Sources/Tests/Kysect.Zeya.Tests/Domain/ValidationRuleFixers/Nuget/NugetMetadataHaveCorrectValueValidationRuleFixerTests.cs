using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.RepositoryValidationRules.Fixers.Nuget;
using Kysect.Zeya.RepositoryValidationRules.Rules.Nuget;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.Nuget;

public class NugetMetadataHaveCorrectValueValidationRuleFixerTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly NugetMetadataHaveCorrectValueValidationRuleFixer _fixer;

    public NugetMetadataHaveCorrectValueValidationRuleFixerTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _fixer = _validationTestFixture.GetRequiredService<NugetMetadataHaveCorrectValueValidationRuleFixer>();
    }

    [Fact]
    public void Fix_SolutionWithoutDirectoryBuildProps_CreateAndAddRequiredProperties()
    {
        var arguments = new NugetMetadataHaveCorrectValueValidationRule.Arguments(new Dictionary<string, string>() { { "Property1", "Value" } });
        var expected = """
                                <Project>
                                  <PropertyGroup>
                                    <Property1>Value</Property1>
                                  </PropertyGroup>
                                </Project>
                                """;

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        var localGithubRepository = _validationTestFixture.CreateLocalRepository();
        _fixer.Fix(arguments, localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expected);
    }
}