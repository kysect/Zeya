using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation.Abstractions;
using Kysect.Zeya.RepositoryValidation.Abstractions.Models;
using Kysect.Zeya.ValidationRules.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules.Nuget;

public class NugetMetadataHaveCorrectValueValidationRule()
    : IScenarioStepExecutor<NugetMetadataHaveCorrectValueValidationRule.Arguments>
{
    [ScenarioStep("Nuget.MetadataHaveCorrectValue")]
    public record Arguments(Dictionary<string, string> RequiredKeyValues) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Nuget.MetadataHaveCorrectValue;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        LocalRepositorySolution repositorySolutionAccessor = repositoryValidationContext.Repository.SolutionManager.GetSolution();

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

        var invalidValues = new List<string>();

        foreach (var (key, value) in request.RequiredKeyValues)
        {
            DotnetProjectProperty? property = directoryBuildPropsFile.File.Properties.FindProperty(key);
            if (property is null)
            {
                invalidValues.Add(key);
                continue;
            }

            if (property.Value.Value != value)
                invalidValues.Add(key);
        }

        if (invalidValues.Any())
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Directory.Build.props options has incorrect value or value is missed: " + invalidValues.ToSingleString(),
                Arguments.DefaultSeverity);
        }
    }
}