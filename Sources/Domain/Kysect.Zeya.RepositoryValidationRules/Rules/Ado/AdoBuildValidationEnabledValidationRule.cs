using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.LocalRepositoryAccess.Ado;

namespace Kysect.Zeya.RepositoryValidationRules.Rules.Ado;

public class AdoBuildValidationEnabledValidationRule
    : IScenarioStepExecutor<AdoBuildValidationEnabledValidationRule.Arguments>
{
    [ScenarioStep("Ado.BuildValidationEnabled")]
    public record Arguments() : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Ado.BuildValidationEnabled;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        if (repositoryValidationContext.Repository is not LocalAdoRepository adoRepository)
        {
            string message = $"Skip {request.DiagnosticCode} because repository doesn't contain ADO remote.";
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(request.DiagnosticCode, message, RepositoryValidationSeverity.RuntimeError);

            return;
        }

        bool buildValidationEnabled = adoRepository.AdoIntegrationService.BuildValidationEnabled(adoRepository.RepositoryUrlParts.OrganizationUrl, adoRepository.RepositoryUrlParts.Project);
        if (!buildValidationEnabled)
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                "Repository build validation is disabled.",
                Arguments.DefaultSeverity);
        }
    }
}