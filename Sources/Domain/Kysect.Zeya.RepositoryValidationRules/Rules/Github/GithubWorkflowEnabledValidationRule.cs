using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;

namespace Kysect.Zeya.RepositoryValidationRules.Rules.Github;

public class GithubWorkflowEnabledValidationRule(IFileSystem fileSystem) : IScenarioStepExecutor<GithubWorkflowEnabledValidationRule.Arguments>
{
    [ScenarioStep("Github.ActionConfigured")]
    public record Arguments(
        [property: Required] IReadOnlyCollection<string> Workflows) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Github.ActionConfigured;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();

        ILocalRepository localRepository = repositoryValidationContext.Repository;
        if (localRepository is not LocalGithubRepository clonedGithubRepository)
        {
            repositoryValidationContext.DiagnosticCollector.AddRuntimeError(
                request.DiagnosticCode,
                $"Cannot apply github validation rule on non github repository",
                Arguments.DefaultSeverity);
            return;
        }

        foreach (string workflow in request.Workflows)
            AnalyzeWorkflow(repositoryValidationContext, clonedGithubRepository, request, workflow);
    }

    private void AnalyzeWorkflow(
        RepositoryValidationContext repositoryValidationContext,
        LocalGithubRepository repository,
        Arguments request,
        string workflow)
    {
        if (!fileSystem.File.Exists(workflow))
        {
            repositoryValidationContext.DiagnosticCollector.AddRuntimeError(
                request.DiagnosticCode,
                $"Master file {workflow} missed",
                Arguments.DefaultSeverity);
            return;
        }
        IFileInfo masterFileInfo = fileSystem.FileInfo.New(workflow);
        string masterFileContent = fileSystem.File.ReadAllText(workflow);

        var expectedPath = repository.GetWorkflowPath(masterFileInfo.Name);
        if (!repository.FileSystem.Exists(expectedPath))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Workflow {masterFileInfo.Name} must be configured",
                Arguments.DefaultSeverity);
            return;
        }

        string repositoryWorkflowFileContent = repository.FileSystem.ReadAllText(expectedPath);
        if (!string.Equals(masterFileContent, repositoryWorkflowFileContent))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Workflow {masterFileInfo.Name} configuration do not match with master file",
                Arguments.DefaultSeverity);
        }
    }
}