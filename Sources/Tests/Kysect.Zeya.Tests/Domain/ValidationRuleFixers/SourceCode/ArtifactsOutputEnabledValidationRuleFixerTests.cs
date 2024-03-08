using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.SourceCode;

public class ArtifactsOutputEnabledValidationRuleFixerTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly ArtifactsOutputEnabledValidationRuleFixer _fixer;

    public ArtifactsOutputEnabledValidationRuleFixerTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _fixer = new ArtifactsOutputEnabledValidationRuleFixer(_validationTestFixture.Formatter, _validationTestFixture.GetLogger<ArtifactsOutputEnabledValidationRuleFixer>());
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
            .Save(_validationTestFixture.FileSystem, _validationTestFixture.CurrentPath, _validationTestFixture.Formatter);

        LocalGithubRepository localGithubRepository = _validationTestFixture.CreateGithubRepository();
        _fixer.Fix(new ArtifactsOutputEnabledValidationRule.Arguments(), localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDirectoryBuildProps);
    }
}