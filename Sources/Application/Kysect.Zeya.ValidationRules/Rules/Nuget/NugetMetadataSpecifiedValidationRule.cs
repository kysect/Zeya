using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.RepositoryValidation.Abstractions;
using Kysect.Zeya.RepositoryValidation.Abstractions.Models;
using Kysect.Zeya.ValidationRules.Abstractions;

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
        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.Repository);

        if (!repositoryValidationContext.Repository.FileSystem.Exists(repositorySolutionAccessor.GetDirectoryBuildPropsPath()))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                ValidationRuleMessages.DirectoryBuildPropsFileMissed,
                Arguments.DefaultSeverity);
            return;
        }

        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();
        DirectoryBuildPropsFile directoryBuildPropsFile = solutionModifier.GetOrCreateDirectoryBuildPropsModifier();

        List<string> missedValues = new List<string>();
        foreach (var requiredField in request.RequiredValues)
        {
            if (directoryBuildPropsFile.File.Properties.FindProperty(requiredField) is null)
                missedValues.Add(requiredField);
        }

        if (missedValues.Any())
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Directory.Build.props file does not contains required option: " + missedValues.ToSingleString(),
                Arguments.DefaultSeverity);
        }
    }
}