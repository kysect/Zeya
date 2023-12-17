using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ProjectSystemIntegration;

namespace Kysect.Zeya.ValidationRules.Rules.Nuget;

public class NugetMetadataSpecifiedValidationRule(RepositorySolutionAccessorFactory repositorySolutionAccessorFactory)
    : IScenarioStepExecutor<NugetMetadataSpecifiedValidationRule.Arguments>
{
    [ScenarioStep("Nuget.MetadataSpecified")]
    public record Arguments(IReadOnlyCollection<string> RequiredValues) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Nuget.MetadataSpecified;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();
        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.RepositoryAccessor);

        if (!repositoryValidationContext.RepositoryAccessor.Exists(repositorySolutionAccessor.GetDirectoryBuildPropsPath()))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                "Directory.Build.props file is not exists.",
                DirectoryBuildPropsContainsRequiredFields.DefaultSeverity);
            return;
        }

        var directoryBuildPropsContent = repositoryValidationContext.RepositoryAccessor.ReadAllText(repositorySolutionAccessor.GetDirectoryBuildPropsPath());
        var directoryBuildPropsParser = new DirectoryBuildPropsParser();
        Dictionary<string, string> buildPropsValues = directoryBuildPropsParser.Parse(directoryBuildPropsContent);

        List<string> missedValues = new List<string>();
        foreach (var requiredField in request.RequiredValues)
        {
            if (!buildPropsValues.ContainsKey(requiredField))
            {
                missedValues.Add(requiredField);
            }
        }

        if (missedValues.Any())
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"Directory.Build.props file does not contains required option: " + missedValues.ToSingleString(),
                Arguments.DefaultSeverity);
        }
    }
}