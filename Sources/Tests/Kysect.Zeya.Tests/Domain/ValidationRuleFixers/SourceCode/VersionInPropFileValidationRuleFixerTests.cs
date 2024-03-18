using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.SourceCode;

public class VersionInPropFileValidationRuleFixerTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly VersionInPropFileValidationRuleFixer _fixer;

    public VersionInPropFileValidationRuleFixerTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _fixer = _validationTestFixture.GetRequiredService<VersionInPropFileValidationRuleFixer>();
    }

    [Fact]
    public void Fix_VersionInCsproj_MoveVersionToPropsFile()
    {
        var expectedProjectContent = """
                                      <Project>
                                        <PropertyGroup>
                                        </PropertyGroup>
                                      </Project>
                                      """;

        var expectedDirectoryBuildPropsFile = """
                                              <Project>
                                                <PropertyGroup>
                                                  <Version>1.2.3.4</Version>
                                                </PropertyGroup>
                                              </Project>
                                              """;

        var dotnetProjectFile = DotnetProjectFile.CreateEmpty();
        dotnetProjectFile
            .Properties
            .AddProperty("Version", "1.2.3.4");

        string projectName = "Project";
        var arguments = new VersionInPropFileValidationRule.Arguments();

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName, dotnetProjectFile))
            .Save(_validationTestFixture.CurrentPath);

        ILocalRepository localRepository = _validationTestFixture.CreateLocalRepository();
        _fixer.Fix(arguments, localRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, projectName, $"{projectName}.csproj")
            .ShouldExists()
            .ShouldHaveContent(expectedProjectContent);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDirectoryBuildPropsFile);
    }
}