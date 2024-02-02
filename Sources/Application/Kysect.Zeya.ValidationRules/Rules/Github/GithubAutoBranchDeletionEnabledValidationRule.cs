using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryAccess;

namespace Kysect.Zeya.ValidationRules.Rules.Github;

public class GithubAutoBranchDeletionEnabledValidationRule(IGithubIntegrationService githubIntegrationService)
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
        GithubRepository? githubRepository = repositoryValidationContext.TryGetGithubMetadata();
        if (githubRepository is null)
        {
            repositoryValidationContext.DiagnosticCollector.AddRuntimeError(
                request.DiagnosticCode,
                $"Skip {request.DiagnosticCode} because repository do not have GitHub metadata.");

            return;
        }

        if (!githubIntegrationService.DeleteBranchOnMerge(githubRepository))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                "Branch deletion on merge must be enabled.",
                Arguments.DefaultSeverity);
        }
    }
}