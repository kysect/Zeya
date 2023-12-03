using System.IO.Abstractions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules.Rules.Github;

public class GithubWorkflowEnabledValidationRule(IFileSystem fileSystem) : IScenarioStepExecutor<GithubWorkflowEnabledValidationRule.Arguments>
{
    [ScenarioStep("Github.BuildWorkflowEnabled")]
    public record Arguments(string MasterFile) : IScenarioStep
    {
        public static string DiagnosticCode => RuleDescription.Github.BuildWorkflowEnabled;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments arguments)
    {
        var repositoryValidationContext = context.GetValidationContext();

        var masterFileInfo = fileSystem.FileInfo.New(arguments.MasterFile);
        var expectedPath = fileSystem.Path.Combine(".github", "workflow", masterFileInfo.Name);

        if (!repositoryValidationContext.RepositoryAccessor.Exists(expectedPath))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                $"Workflow {masterFileInfo.Name} must be configured",
                Arguments.DefaultSeverity);
            return;
        }

        var masterFileContent = fileSystem.File.ReadAllText(arguments.MasterFile);
        var originalFIleContent = repositoryValidationContext.RepositoryAccessor.ReadFile(expectedPath);

        if (string.Equals(masterFileContent, originalFIleContent))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                $"Workflow {masterFileInfo.Name} has unexpected configuration",
                Arguments.DefaultSeverity);
        }
    }
}