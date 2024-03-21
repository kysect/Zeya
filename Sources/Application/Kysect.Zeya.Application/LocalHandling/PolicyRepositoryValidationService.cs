using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Application.LocalHandling;

public class PolicyRepositoryValidationService(
    IGithubRepositoryProvider githubRepositoryProvider,
    RepositoryValidationService policyRepositoryValidationService,
    ILogger<PolicyRepositoryValidationService> logger) : IPolicyRepositoryValidationService
{
    public void CreatePullRequestWithFix(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        policyRepositoryValidationService.CreatePullRequestWithFix(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenario);
    }

    public void AnalyzerAndFix(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        policyRepositoryValidationService.AnalyzerAndFix(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenario);
    }

    public void Analyze(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        policyRepositoryValidationService.AnalyzeSingleRepository(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenario);
    }
}