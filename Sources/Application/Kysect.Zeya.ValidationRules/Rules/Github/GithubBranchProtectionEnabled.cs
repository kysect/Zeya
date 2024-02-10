using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration.Abstraction.Contracts;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.Models;
using Kysect.Zeya.ValidationRules.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules.Github;

public class GithubBranchProtectionEnabledValidationRule(IGithubIntegrationService githubIntegrationService) : IScenarioStepExecutor<GithubBranchProtectionEnabledValidationRule.Arguments>
{
    [ScenarioStep("Github.BranchProtectionEnabled")]
    public record Arguments(
        bool PullRequestReviewRequired,
        bool ConversationResolutionRequired) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Github.BranchProtectionEnabled;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();

        string branch = ValidationConstants.DefaultBranch;
        if (repositoryValidationContext.Repository is not ClonedGithubRepository clonedGithubRepository)
        {
            repositoryValidationContext.DiagnosticCollector.AddRuntimeError(
                request.DiagnosticCode,
                $"Skip {request.DiagnosticCode} because repository do not have GitHub metadata.");

            return;
        }

        RepositoryBranchProtection repositoryBranchProtection = githubIntegrationService.GetRepositoryBranchProtection(clonedGithubRepository.GithubMetadata, branch);

        if (request.PullRequestReviewRequired && !repositoryBranchProtection.PullRequestReviewsRequired)
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Pull request review must be enabled enabled for {branch}.",
                Arguments.DefaultSeverity);
        }

        if (request.ConversationResolutionRequired && !repositoryBranchProtection.ConversationResolutionRequired)
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Conversation resolution must be required for {branch}.",
                Arguments.DefaultSeverity);
        }
    }
}