using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidationRules.Fixers.Github;
using Kysect.Zeya.RepositoryValidationRules.Rules.Github;
using Kysect.Zeya.Tests.Domain.ValidationRules;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.Github;

public class GithubWorkflowEnabledValidationRuleFixerTests : ValidationRuleTestBase
{
    private readonly GithubWorkflowEnabledValidationRuleFixer _fixer;

    public GithubWorkflowEnabledValidationRuleFixerTests()
    {
        _fixer = new GithubWorkflowEnabledValidationRuleFixer(FileSystem, GetLogger<GithubWorkflowEnabledValidationRuleFixer>());
    }

    [Fact]
    public void Fix_NonGithubRepository_SkipFixing()
    {
        var masterFileContent = """
                                - name
                                  - run
                                    - step
                                """;

        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);
        FileSystem.AddFile("build.yaml", new MockFileData(masterFileContent));

        var nonGitRepository = new LocalRepository(CurrentPath, FileSystem);
        _fixer.Fix(new GithubWorkflowEnabledValidationRule.Arguments("build.yaml"), nonGitRepository);
    }

    [Fact]
    public void Fix_SolutionWithoutWorkflow_CreateWorkflowFile()
    {
        var masterFileContent = """
                                          - name
                                            - run
                                              - step
                                          """;

        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);
        FileSystem.AddFile("build.yaml", new MockFileData(masterFileContent));

        _fixer.Fix(new GithubWorkflowEnabledValidationRule.Arguments("build.yaml"), Repository);

        FileSystemAsserts
            .File(CurrentPath, ".github", "workflows", "build.yaml")
            .ShouldExists()
            .ShouldHaveContent(masterFileContent);
    }
}