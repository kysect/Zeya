using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ProjectSystemIntegration;

namespace Kysect.Zeya.ValidationRules.Rules;

[ScenarioStep("DirectoryBuildPropsContainsRequiredFields")]
public record DirectoryBuildPropsContainsRequiredFields(IReadOnlyCollection<string> RequiredFields) : IValidationRule
{
    public string DiagnosticCode => "SRC00007";

    public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
}

public class DirectoryBuildPropsContainsRequiredFieldsValidationRule : IScenarioStepExecutor<DirectoryBuildPropsContainsRequiredFields>
{
    public void Execute(ScenarioContext context, DirectoryBuildPropsContainsRequiredFields request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();

        if (!repositoryValidationContext.RepositoryAccessor.Exists(ValidationConstants.DirectoryBuildPropsPath))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                "Directory.Build.props file is not exists.",
                DirectoryBuildPropsContainsRequiredFields.DefaultSeverity);
            return;
        }

        var directoryBuildPropsContent = repositoryValidationContext.RepositoryAccessor.ReadFile(ValidationConstants.DirectoryBuildPropsPath);
        var directoryBuildPropsParser = new DirectoryBuildPropsParser();
        Dictionary<string, string> buildPropsValues = directoryBuildPropsParser.Parse(directoryBuildPropsContent);

        foreach (var requiredField in request.RequiredFields)
        {
            if (!buildPropsValues.ContainsKey(requiredField))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    request.DiagnosticCode,
                    $"Directory.Build.props field {requiredField} is missed.",
                    DirectoryBuildPropsContainsRequiredFields.DefaultSeverity);
            }
        }
    }
}