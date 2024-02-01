using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.Tests.ValidationRules;
using Kysect.Zeya.ValidationRules.Fixers.SourceCode;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;

namespace Kysect.Zeya.Tests.ValidationRuleFixers.SourceCode;

public class ArtifactsOutputEnabledValidationRuleFixerTests : ValidationRuleTestBase
{
    private readonly ArtifactsOutputEnabledValidationRuleFixer _fixer;

    public ArtifactsOutputEnabledValidationRuleFixerTests()
    {
        _fixer = new ArtifactsOutputEnabledValidationRuleFixer(RepositorySolutionAccessorFactory, Formatter, Logger);
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