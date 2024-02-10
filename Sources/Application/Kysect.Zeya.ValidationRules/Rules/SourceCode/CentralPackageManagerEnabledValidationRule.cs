using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.RepositoryValidation.Abstractions;
using Kysect.Zeya.RepositoryValidation.Abstractions.Models;
using Kysect.Zeya.ValidationRules.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class CentralPackageManagerEnabledValidationRule(RepositorySolutionAccessorFactory repositorySolutionAccessorFactory)
    : IScenarioStepExecutor<CentralPackageManagerEnabledValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.CentralPackageManagerEnabled")]
    public record Arguments : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.CentralPackageManagerEnabled;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
        public const string CentralPackageManagementDisabledMessage = "The repository does not use Central Package Manager";
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.Repository);
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();

        bool artifactsOutputEnabled = solutionModifier.GetOrCreateDirectoryBuildPropsModifier().ArtifactsOutputEnabled();
        if (!artifactsOutputEnabled)
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                Arguments.CentralPackageManagementDisabledMessage,
                Arguments.DefaultSeverity);
        }
    }
}