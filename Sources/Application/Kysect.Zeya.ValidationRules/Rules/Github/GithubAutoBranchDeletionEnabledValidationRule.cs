using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Kysect.Zeya.ValidationRules.Rules.Github;

public class GithubAutoBranchDeletionEnabledValidationRule(IGitHubClient githubClient, ILogger logger) : IScenarioStepExecutor<GithubAutoBranchDeletionEnabledValidationRule.Arguments>
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
            logger.LogInformation("Skip {Rule} because repository do not have GitHub metadata", request.DiagnosticCode);
            return;
        }

        Repository? repositoryInfo = githubClient.Repository.Get(githubRepository.Owner, githubRepository.Name).Result;
        if (repositoryInfo.DeleteBranchOnMerge is null or false)
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                "Branch deletion on merge must be enabled.",
                Arguments.DefaultSeverity);
        }
    }
}