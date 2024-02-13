using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.RepositoryValidationRules.Fixers.Nuget;
using Kysect.Zeya.RepositoryValidationRules.Rules.Nuget;
using Kysect.Zeya.Tests.Domain.ValidationRules;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.Nuget;

public class NugetMetadataHaveCorrectValueValidationRuleFixerTests : ValidationRuleTestBase
{
    private readonly NugetMetadataHaveCorrectValueValidationRuleFixer _fixer;

    public NugetMetadataHaveCorrectValueValidationRuleFixerTests()
    {
        _fixer = new NugetMetadataHaveCorrectValueValidationRuleFixer(Formatter, Logger);
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
            .Save(FileSystem, CurrentPath, Formatter);

        _fixer.Fix(arguments, Repository);

        FileSystemAsserts
            .File(CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expected);
    }
}