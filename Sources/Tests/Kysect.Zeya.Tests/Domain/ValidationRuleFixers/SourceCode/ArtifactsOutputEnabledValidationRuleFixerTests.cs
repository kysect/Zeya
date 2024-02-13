using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Domain.ValidationRules;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.SourceCode;

public class ArtifactsOutputEnabledValidationRuleFixerTests : ValidationRuleTestBase
{
    private readonly ArtifactsOutputEnabledValidationRuleFixer _fixer;

    public ArtifactsOutputEnabledValidationRuleFixerTests()
    {
        _fixer = new ArtifactsOutputEnabledValidationRuleFixer(Formatter, Logger);
    }

    [Fact]
    public void Fix_SolutionWithoutDirectoryBuildProps_CreateWithArtifactOutput()
    {
        var expectedDirectoryBuildProps = """
                                          <Project>
                                            <PropertyGroup>
                                              <UseArtifactsOutput>true</UseArtifactsOutput>
                                            </PropertyGroup>
                                          </Project>
                                          """;

        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);

        _fixer.Fix(new ArtifactsOutputEnabledValidationRule.Arguments(), Repository);

        FileSystemAsserts
            .File(CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDirectoryBuildProps);
    }
}