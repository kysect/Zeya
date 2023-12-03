using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ProjectSystemIntegration;

namespace Kysect.Zeya.ValidationRules.Rules;

[ScenarioStep("DirectoryBuildPropsContainsRequiredFields")]
public record DirectoryBuildPropsContainsRequiredFields(IReadOnlyCollection<string> RequiredFields) : IScenarioStep
{
    public static string DiagnosticCode = "SRC00007";
    public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;

    public static string GetMessage(DirectoryBuildPropsContainsRequiredFields request, string field)
    {
        return $"Directory.Build.props field {field} is missed.";
    }
}

public class DirectoryBuildPropsContainsRequiredFieldsValidationRule : IScenarioStepExecutor<DirectoryBuildPropsContainsRequiredFields>
{
    public void Execute(ScenarioContext context, DirectoryBuildPropsContainsRequiredFields request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        if (!repositoryValidationContext.RepositoryAccessor.Exists(ValidationConstants.DirectoryPackagePropsPath))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                DirectoryBuildPropsContainsRequiredFields.DiagnosticCode,
                "Directory.Build.props file is not exists.",
                DirectoryBuildPropsContainsRequiredFields.DefaultSeverity);
            return;
        }

        var directoryBuildPropsContent = repositoryValidationContext.RepositoryAccessor.ReadFile(ValidationConstants.DirectoryPackagePropsPath);
        var directoryBuildPropsParser = new DirectoryBuildPropsParser();
        Dictionary<string, string> buildPropsValues = directoryBuildPropsParser.Parse(directoryBuildPropsContent);

        foreach (var requiredField in request.RequiredFields)
        {
            if (!buildPropsValues.ContainsKey(requiredField))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    DirectoryBuildPropsContainsRequiredFields.DiagnosticCode,
                    DirectoryBuildPropsContainsRequiredFields.GetMessage(request, requiredField),
                    DirectoryBuildPropsContainsRequiredFields.DefaultSeverity);
            }
        }
    }
}