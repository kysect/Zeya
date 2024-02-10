using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.Models;
using Kysect.Zeya.ValidationRules.Abstractions;
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

        if (!fileSystem.File.Exists(request.MasterFile))
        {
            repositoryValidationContext.DiagnosticCollector.AddRuntimeError(
                request.DiagnosticCode,
                $"Master file {request.MasterFile} missed",
                Arguments.DefaultSeverity);
            return;
        }
        IFileInfo masterFileInfo = fileSystem.FileInfo.New(request.MasterFile);
        string masterFileContent = fileSystem.File.ReadAllText(request.MasterFile);

        var expectedPath = repositoryValidationContext.Repository.GetWorkflowPath(masterFileInfo.Name);
        if (!repositoryValidationContext.Repository.Exists(expectedPath))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Workflow {masterFileInfo.Name} must be configured",
                Arguments.DefaultSeverity);
            return;
        }

        string repositoryWorkflowFileContent = repositoryValidationContext.Repository.ReadAllText(expectedPath);
        if (!string.Equals(masterFileContent, repositoryWorkflowFileContent))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Workflow {masterFileInfo.Name} configuration do not match with master file",
                Arguments.DefaultSeverity);
        }
    }
}