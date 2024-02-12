using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;

namespace Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;

public class CentralPackageManagerEnabledValidationRule()
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
        LocalRepositorySolution repositorySolutionAccessor = repositoryValidationContext.Repository.SolutionManager.GetSolution();
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();

        bool cpmEnabled = solutionModifier.GetOrCreateDirectoryPackagePropsModifier().GetCentralPackageManagement();
        if (!cpmEnabled)
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                Arguments.CentralPackageManagementDisabledMessage,
                Arguments.DefaultSeverity);
        }
    }
}