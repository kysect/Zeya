using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules.Github;

public class GithubWorkflowEnabledValidationRule(IFileSystem fileSystem) : IScenarioStepExecutor<GithubWorkflowEnabledValidationRule.Arguments>
{
    [ScenarioStep("Github.BuildWorkflowEnabled")]
    public record Arguments(string MasterFile) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Github.BuildWorkflowEnabled;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();

        var masterFileInfo = fileSystem.FileInfo.New(request.MasterFile);
        var expectedPath = fileSystem.Path.Combine(".github", "workflow", masterFileInfo.Name);

        if (!repositoryValidationContext.RepositoryAccessor.Exists(expectedPath))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"Workflow {masterFileInfo.Name} must be configured",
                Arguments.DefaultSeverity);
            return;
        }

        var masterFileContent = fileSystem.File.ReadAllText(request.MasterFile);
        var originalFIleContent = repositoryValidationContext.RepositoryAccessor.ReadFile(expectedPath);

        if (string.Equals(masterFileContent, originalFIleContent))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"Workflow {masterFileInfo.Name} has unexpected configuration",
                Arguments.DefaultSeverity);
        }
    }
}