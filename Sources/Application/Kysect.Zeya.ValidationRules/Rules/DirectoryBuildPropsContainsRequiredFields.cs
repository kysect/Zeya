using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules.Rules;

public class DirectoryBuildPropsContainsRequiredFieldsValidationRule(RepositorySolutionAccessorFactory repositorySolutionAccessorFactory)
    : IScenarioStepExecutor<DirectoryBuildPropsContainsRequiredFieldsValidationRule.Arguments>
{
    [ScenarioStep("DirectoryBuildPropsContainsRequiredFields")]
    public record Arguments(IReadOnlyCollection<string> RequiredFields) : IValidationRule
    {
        public string DiagnosticCode => "SRC00007";

        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.Repository);

        if (!repositoryValidationContext.Repository.Exists(repositorySolutionAccessor.GetDirectoryBuildPropsPath()))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                ValidationRuleMessages.DirectoryBuildPropsFileMissed,
                Arguments.DefaultSeverity);
            return;
        }

        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();
        DirectoryBuildPropsFile directoryBuildPropsFile = solutionModifier.GetOrCreateDirectoryBuildPropsModifier();

        foreach (var requiredField in request.RequiredFields)
        {
            if (directoryBuildPropsFile.File.Properties.FindProperty(requiredField) is null)
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    request.DiagnosticCode,
                    $"Directory.Build.props field {requiredField} is missed.",
                    Arguments.DefaultSeverity);
            }
        }
    }
}