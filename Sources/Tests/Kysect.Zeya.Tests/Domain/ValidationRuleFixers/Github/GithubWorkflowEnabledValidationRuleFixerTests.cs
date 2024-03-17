using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidationRules.Fixers.Github;
using Kysect.Zeya.RepositoryValidationRules.Rules.Github;
using Kysect.Zeya.Tests.Tools;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.Github;

public class GithubWorkflowEnabledValidationRuleFixerTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly GithubWorkflowEnabledValidationRuleFixer _fixer;

    public GithubWorkflowEnabledValidationRuleFixerTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _fixer = _validationTestFixture.GetRequiredService<GithubWorkflowEnabledValidationRuleFixer>();
    }

    [Fact]
    public void Fix_NonGithubRepository_SkipFixing()
    {
        var masterFileContent = """
                                - name
                                  - run
                                    - step
                                """;

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        _validationTestFixture.FileSystem.AddFile("build.yaml", new MockFileData(masterFileContent));

        ILocalRepository nonGitRepository = _validationTestFixture.CreateLocalRepository();
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

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        _validationTestFixture.FileSystem.AddFile("build.yaml", new MockFileData(masterFileContent));
        LocalGithubRepository localGithubRepository = _validationTestFixture.CreateGithubRepository();

        _fixer.Fix(new GithubWorkflowEnabledValidationRule.Arguments("build.yaml"), localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, ".github", "workflows", "build.yaml")
            .ShouldExists()
            .ShouldHaveContent(masterFileContent);
    }
}