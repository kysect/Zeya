using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess.Github;
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
        _fixer = new NugetMetadataHaveCorrectValueValidationRuleFixer(_validationTestFixture.Formatter, _validationTestFixture.GetLogger<NugetMetadataHaveCorrectValueValidationRuleFixer>());
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

        new SolutionFileStructureBuilder("Solution")
            .Save(_validationTestFixture.FileSystem, _validationTestFixture.CurrentPath, _validationTestFixture.Formatter);

        LocalGithubRepository localGithubRepository = _validationTestFixture.CreateGithubRepository();
        _fixer.Fix(arguments, localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expected);
    }
}