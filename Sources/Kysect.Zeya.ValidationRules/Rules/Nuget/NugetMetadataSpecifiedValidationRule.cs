using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ProjectSystemIntegration;

namespace Kysect.Zeya.ValidationRules.Rules.Nuget;

public class NugetMetadataSpecifiedValidationRule : IScenarioStepExecutor<NugetMetadataSpecifiedValidationRule.Arguments>
{
    [ScenarioStep("Nuget.MetadataSpecified")]
    public record Arguments(Dictionary<string, string> RequiredKeyValues, IReadOnlyCollection<string> RequiredValues) : IScenarioStep
    {
        public static string DiagnosticCode => RuleDescription.Nuget.MetadataSpecified;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
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

        foreach (var requiredField in request.RequiredValues)
        {
            if (!buildPropsValues.ContainsKey(requiredField))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    Arguments.DiagnosticCode,
                    $"Directory.Build.props file does not contains required option: {requiredField}",
                    Arguments.DefaultSeverity);
            }
        }

        foreach (var (key, value) in request.RequiredKeyValues)
        {
            if (!buildPropsValues.TryGetValue(key, out var specifiedValue))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    Arguments.DiagnosticCode,
                    $"Directory.Build.props file does not contains required option: {key}",
                    Arguments.DefaultSeverity);
                continue;
            }

            if (specifiedValue != value)
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    Arguments.DiagnosticCode,
                    $"Directory.Build.props option {key} expected to be {value} but was {specifiedValue}",
                    Arguments.DefaultSeverity);
            }
        }
    }
}