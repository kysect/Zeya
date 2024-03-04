using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.RepositoryValidation;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Client.Local;

public class RepositoryValidationLocalClient(
    IGithubRepositoryProvider githubRepositoryProvider,
    RepositoryValidationService repositoryValidationService,
    RepositoryValidationRuleProvider validationRuleProvider,
    ILogger<RepositoryValidationLocalClient> logger) : IRepositoryValidationApi
{
    public void CreatePullRequestWithFix(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        ScenarioContent scenarioContent = validationRuleProvider.ReadScenario(scenario);
        repositoryValidationService.CreatePullRequestWithFix(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenarioContent);
    }

    public void AnalyzerAndFix(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        ScenarioContent scenarioContent = validationRuleProvider.ReadScenario(scenario);
        repositoryValidationService.AnalyzerAndFix(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenarioContent);
    }

    public void Analyze(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        ScenarioContent scenarioContent = validationRuleProvider.ReadScenario(scenario);
        repositoryValidationService.AnalyzeSingleRepository(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenarioContent);
    }
}