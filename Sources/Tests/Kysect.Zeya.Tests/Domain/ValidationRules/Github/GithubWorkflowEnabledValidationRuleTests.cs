using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.Github;
using Kysect.Zeya.Tests.Tools;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.Github;

public class GithubWorkflowEnabledValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly GithubWorkflowEnabledValidationRule _validationRule;

    public GithubWorkflowEnabledValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _validationRule = new GithubWorkflowEnabledValidationRule(_validationTestFixture.FileSystem);
    }

    [Fact]
    public void Execute_NotGithubRepository_ReturnErrorAboutIncorrectRepository()
    {
        var arguments = new GithubWorkflowEnabledValidationRule.Arguments("Master.yaml");

        var notGithubContext = RepositoryValidationContextExtensions.CreateScenarioContext(new RepositoryValidationContext(new LocalRepository(_validationTestFixture.CurrentPath, _validationTestFixture.FileSystem), _validationTestFixture.DiagnosticCollectorAsserts.GetCollector()));
        _validationRule.Execute(notGithubContext, arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(1)
            .ShouldHaveError(1, arguments.DiagnosticCode, "Cannot apply github validation rule on non github repository");
    }

    [Fact]
    public void Execute_MissedMasterFile_ReturnErrorAboutMissedFile()
    {
        var arguments = new GithubWorkflowEnabledValidationRule.Arguments("Master.yaml");

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(1)
            .ShouldHaveError(1, arguments.DiagnosticCode, "Master file Master.yaml missed");
    }

    [Fact]
    public void Execute_WorkflowFileIsNotExists_ReturnDiagnosticAboutMissedFile()
    {
        string masterYamlFilePath = "build.yaml";
        var arguments = new GithubWorkflowEnabledValidationRule.Arguments(masterYamlFilePath);
        _validationTestFixture.FileSystem.AddFile(masterYamlFilePath, new MockFileData(string.Empty));

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
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
        string repositoryWorkflowFilePath = _validationTestFixture.FileSystem.Path.Combine(_validationTestFixture.CurrentPath, ".github", "workflows", "build.yaml");

        _validationTestFixture.FileSystem.AddFile(masterYamlFilePath, new MockFileData(masterYamlFileContent));
        _validationTestFixture.FileSystem.AddFile(repositoryWorkflowFilePath, new MockFileData(masterYamlFileContent));

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
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
        string repositoryWorkflowFilePath = _validationTestFixture.FileSystem.Path.Combine(_validationTestFixture.CurrentPath, ".github", "workflows", "build.yaml");

        _validationTestFixture.FileSystem.AddFile(masterYamlFilePath, new MockFileData(masterYamlFileContent));
        _validationTestFixture.FileSystem.AddFile(repositoryWorkflowFilePath, new MockFileData(repositoryYamlFileContent));

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Workflow build.yaml configuration do not match with master file");
    }
}