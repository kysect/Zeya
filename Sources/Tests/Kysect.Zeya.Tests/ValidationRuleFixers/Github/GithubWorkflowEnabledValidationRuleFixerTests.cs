using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.Zeya.Tests.ValidationRules;
using Kysect.Zeya.ValidationRules.Fixers.Github;
using Kysect.Zeya.ValidationRules.Rules.Github;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRuleFixers.Github;

public class GithubWorkflowEnabledValidationRuleFixerTests : ValidationRuleTestBase
{
    private readonly GithubWorkflowEnabledValidationRuleFixer _fixer;

    public GithubWorkflowEnabledValidationRuleFixerTests()
    {
        _fixer = new GithubWorkflowEnabledValidationRuleFixer(FileSystem, Logger);
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