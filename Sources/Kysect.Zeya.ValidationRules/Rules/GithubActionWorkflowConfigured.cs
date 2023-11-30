using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules;

[ScenarioStep("GithubActionWorkflowConfigured")]
public record GithubActionWorkflowConfigured(string FileName) : IScenarioStep
{
    public static string DiagnosticCode = "SRC00006";
    public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;

    public static string GetMessage(GithubActionWorkflowConfigured request)
    {
        return $"Github workflow {request.FileName} was not found.";
    }
}

public class GithubActionWorkflowConfiguredValidationRule : IScenarioStepExecutor<GithubActionWorkflowConfigured>
{
    private readonly IFileSystem _fileSystem;

    public GithubActionWorkflowConfiguredValidationRule(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void Execute(ScenarioContext context, GithubActionWorkflowConfigured request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        var workflowPath = Path.Combine(repositoryValidationContext.RepositoryAccessor.GetFullPath(), ".github", "workflows", request.FileName);

        if (!_fileSystem.File.Exists(workflowPath))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                GithubActionWorkflowConfigured.DiagnosticCode,
                GithubActionWorkflowConfigured.GetMessage(request),
                GithubActionWorkflowConfigured.DefaultSeverity);
        }
    }
}