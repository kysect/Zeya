using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Octokit;

namespace Kysect.Zeya.ValidationRules.Rules;

[ScenarioStep("GithubBranchProtectionEnabled")]
public record GithubBranchProtectionEnabled(
    bool PullRequestReviewRequired,
    bool ConversationResolutionRequired) : IScenarioStep
{
    public static string DiagnosticCode = "SRC00009";
    public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;

    public static string GetMessage(GithubBranchProtectionEnabled request, string message)
    {
        return $"Github branch protections is non configured properly: " + message;
    }
}

public class GithubBranchProtectionEnabledValidationRule : IScenarioStepExecutor<GithubBranchProtectionEnabled>
{
    private readonly GitHubClient _githubClient;

    public GithubBranchProtectionEnabledValidationRule(GitHubClient githubClient)
    {
        _githubClient = githubClient;
    }

    public void Execute(ScenarioContext context, GithubBranchProtectionEnabled request)
    {
        var repositoryValidationContext = context.GetValidationContext();
        // TODO: move to config
        const string defaultBranch = "master";
        GithubRepository repository = repositoryValidationContext.RepositoryAccessor.Repository;
        BranchProtectionSettings? branchProtectionSettings = TryGetBranchProtection(repository, defaultBranch);

        if (branchProtectionSettings is null)
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                GithubBranchProtectionEnabled.DiagnosticCode,
                GithubBranchProtectionEnabled.GetMessage(request, $"Branch protection is disabled for {defaultBranch}."),
                GithubBranchProtectionEnabled.DefaultSeverity);

            return;
        }

        if (request.PullRequestReviewRequired && branchProtectionSettings.RequiredPullRequestReviews is null)
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                GithubBranchProtectionEnabled.DiagnosticCode,
                GithubBranchProtectionEnabled.GetMessage(request, "Pull request review is not enabled."),
                GithubBranchProtectionEnabled.DefaultSeverity);
        }

        if (request.ConversationResolutionRequired && (branchProtectionSettings.RequiredConversationResolution is null || !branchProtectionSettings.RequiredConversationResolution.Enabled))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                GithubBranchProtectionEnabled.DiagnosticCode,
                GithubBranchProtectionEnabled.GetMessage(request, "Conversation resolution must be required."),
                GithubBranchProtectionEnabled.DefaultSeverity);
        }
    }

    private BranchProtectionSettings? TryGetBranchProtection(GithubRepository repository, string defaultBranch)
    {
        try
        {
            return _githubClient.Repository.Branch.GetBranchProtection(repository.Owner, repository.Name, defaultBranch).Result;
        }
        catch (Exception e)
        {
            // TODO: rework this. Possible exception: NotFound, Forbidden (for private repo)
            return null;
        }
    }
}