using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Kysect.Zeya.ValidationRules.Rules.Github;

public class GithubAutoBranchDeletionEnabledValidationRule(IGitHubClient githubClient, ILogger logger) : IScenarioStepExecutor<GithubAutoBranchDeletionEnabledValidationRule.Arguments>
{
    [ScenarioStep("Github.AutoBranchDeletionEnabled")]
    public record Arguments() : IScenarioStep
    {
        public static string DiagnosticCode => RuleDescription.Github.BranchProtectionEnabled;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        string branch = ValidationConstants.DefaultBranch;
        GithubRepository repository = repositoryValidationContext.RepositoryAccessor.Repository;

        var repositoryInfo = githubClient.Repository.Get(repository.Owner, repository.Name).Result;
        if (repositoryInfo.DeleteBranchOnMerge is null or false)
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                "Branch deletion on merge must be enabled.",
                Arguments.DefaultSeverity);
        }
    }
}