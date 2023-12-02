using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Kysect.Zeya.ValidationRules.Rules.Github;

public class GithubBranchProtectionEnabledValidationRule(IGitHubClient githubClient, ILogger logger) : IScenarioStepExecutor<GithubBranchProtectionEnabledValidationRule.Arguments>
{
    [ScenarioStep("Github.BranchProtectionEnabled")]
    public record Arguments(
        bool PullRequestReviewRequired,
        bool ConversationResolutionRequired) : IScenarioStep
    {
        public static string DiagnosticCode => RuleDescription.Github.BranchProtectionEnabled;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        string branch = ValidationConstants.DefaultBranch;
        GithubRepository repository = repositoryValidationContext.RepositoryAccessor.Repository;

        BranchProtectionSettings? branchProtectionSettings = TryGetBranchProtection(repository, branch);

        if (branchProtectionSettings is null)
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                $"Github branch protections for {branch} is non configured.",
                Arguments.DefaultSeverity);

            return;
        }

        if (request.PullRequestReviewRequired && branchProtectionSettings.RequiredPullRequestReviews is null)
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                $"Pull request review must be enabled enabled for {branch}.",
                Arguments.DefaultSeverity);
        }

        if (request.ConversationResolutionRequired && (branchProtectionSettings.RequiredConversationResolution is null || !branchProtectionSettings.RequiredConversationResolution.Enabled))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                $"Conversation resolution must be required for {branch}.",
                Arguments.DefaultSeverity);
        }
    }

    private BranchProtectionSettings? TryGetBranchProtection(GithubRepository repository, string defaultBranch)
    {
        try
        {
            return githubClient.Repository.Branch.GetBranchProtection(repository.Owner, repository.Name, defaultBranch).Result;
        }
        catch (Exception e)
        {
            // TODO: rework this. Possible exception: NotFound, Forbidden (for private repo)
            logger.LogWarning("Failed to get branch protection info for {Repository}: {Message}", repository.FullName, e.Message);
            return null;
        }
    }
}