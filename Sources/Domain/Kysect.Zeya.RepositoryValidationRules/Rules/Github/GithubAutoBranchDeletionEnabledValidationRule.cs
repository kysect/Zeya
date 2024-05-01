using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

namespace Kysect.Zeya.RepositoryValidationRules.Rules.Github;

public class GithubAutoBranchDeletionEnabledValidationRule()
    : IScenarioStepExecutor<GithubAutoBranchDeletionEnabledValidationRule.Arguments>
{
    [ScenarioStep("Github.AutoBranchDeletionEnabled")]
    public record Arguments() : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Github.AutoBranchDeletionEnabled;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        if (repositoryValidationContext.Repository is not LocalGithubRepository clonedGithubRepository)
        {
            string message = $"Skip {request.DiagnosticCode} because repository do not have GitHub metadata.";
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(request.DiagnosticCode, message, RepositoryValidationSeverity.RuntimeError);

            return;
        }

        var deleteBranchOnMerge = clonedGithubRepository.GitHubIntegrationService.DeleteBranchOnMerge(clonedGithubRepository.GithubMetadata).Result;
        if (!deleteBranchOnMerge)
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                "Branch deletion on merge must be enabled.",
                Arguments.DefaultSeverity);
        }
    }
}