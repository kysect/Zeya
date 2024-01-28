using Kysect.Zeya.ValidationRules.Rules.Github;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRules.Github;

public class GithubWorkflowEnabledValidationRuleTests : ValidationRuleTestBase
{
    private readonly GithubWorkflowEnabledValidationRule _validationRule;

    public GithubWorkflowEnabledValidationRuleTests()
    {
        _validationRule = new GithubWorkflowEnabledValidationRule(FileSystem);
    }

    [Fact]
    public void Execute_MissedMasterFile_ReturnErrorAboutMissedFile()
    {
        var arguments = new GithubWorkflowEnabledValidationRule.Arguments("Master.yaml");

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(1)
            .ShouldHaveError(1, arguments.DiagnosticCode, "Master file Master.yaml missed");
    }

    [Fact]
    public void Execute_WorkflowFileIsNotExists_ReturnDiagnosticAboutMissedFile()
    {
        string masterYamlFilePath = "build.yaml";
        var arguments = new GithubWorkflowEnabledValidationRule.Arguments(masterYamlFilePath);
        FileSystem.AddFile(masterYamlFilePath, new MockFileData(string.Empty));

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Workflow build.yaml must be configured");
    }

    [Fact]
    public void Execute_WorkflowFileSameWithMaster_ReturnNoDiagnostics()
    {
        string masterYamlFilePath = "build.yaml";
        string masterYamlFileContent = """
                                       - step
                                         - run
                                         - run
                                       """;

        var arguments = new GithubWorkflowEnabledValidationRule.Arguments(masterYamlFilePath);
        string repositoryWorkflowFilePath = FileSystem.Path.Combine(CurrentPath, ".github", "workflows", "build.yaml");

        FileSystem.AddFile(masterYamlFilePath, new MockFileData(masterYamlFileContent));
        FileSystem.AddFile(repositoryWorkflowFilePath, new MockFileData(masterYamlFileContent));

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }

    [Fact]
    public void Execute_WorkflowFileDoNotMatchWithMaster_ReturnDiagnosticAboutMismatch()
    {
        string masterYamlFilePath = "build.yaml";
        string masterYamlFileContent = """
                                       - step
                                         - run
                                         - run
                                       """;

        string repositoryYamlFileContent = """
                                       - step
                                         - run
                                       """;

        var arguments = new GithubWorkflowEnabledValidationRule.Arguments(masterYamlFilePath);
        string repositoryWorkflowFilePath = FileSystem.Path.Combine(CurrentPath, ".github", "workflows", "build.yaml");

        FileSystem.AddFile(masterYamlFilePath, new MockFileData(masterYamlFileContent));
        FileSystem.AddFile(repositoryWorkflowFilePath, new MockFileData(repositoryYamlFileContent));

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Workflow build.yaml configuration do not match with master file");
    }
}