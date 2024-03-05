using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.RepositoryValidation;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Application.LocalHandling;

public class PolicyRepositoryValidationService(
    IGithubRepositoryProvider githubRepositoryProvider,
    RepositoryValidationService policyRepositoryValidationService,
    RepositoryValidationRuleProvider validationRuleProvider,
    ILogger<PolicyRepositoryValidationService> logger) : IPolicyRepositoryValidationService
{
    public void CreatePullRequestWithFix(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        ScenarioContent scenarioContent = validationRuleProvider.ReadScenario(scenario);
        policyRepositoryValidationService.CreatePullRequestWithFix(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenarioContent);
    }

    public void AnalyzerAndFix(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        ScenarioContent scenarioContent = validationRuleProvider.ReadScenario(scenario);
        policyRepositoryValidationService.AnalyzerAndFix(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenarioContent);
    }

    public void Analyze(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        ScenarioContent scenarioContent = validationRuleProvider.ReadScenario(scenario);
        policyRepositoryValidationService.AnalyzeSingleRepository(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenarioContent);
    }
}