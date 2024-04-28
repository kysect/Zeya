using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Application.LocalHandling;

public class PolicyRepositoryValidationService(
    ValidationRuleParser validationRuleParser,
    RepositoryValidationProcessingAction validationProcessingAction,
    IRepositoryValidationReporter reporter,
    IGithubRepositoryProvider githubRepositoryProvider,
    RepositoryValidationService policyRepositoryValidationService,
    ILogger<PolicyRepositoryValidationService> logger) : IPolicyRepositoryValidationService
{
    public async Task CreatePullRequestWithFix(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        LocalGithubRepository localGithubRepository = githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name);
        RepositoryValidationReport report = policyRepositoryValidationService.Analyze([localGithubRepository], scenario);
        IReadOnlyCollection<string> validationRuleCodeForFix = report.GetAllDiagnosticRuleCodes();
        await policyRepositoryValidationService.CreatePullRequestWithFix(localGithubRepository, scenario, validationRuleCodeForFix);
    }

    public void AnalyzerAndFix(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        policyRepositoryValidationService.AnalyzerAndFix(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenario);
    }

    public void Analyze(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        IReadOnlyCollection<IValidationRule> validationRules = validationRuleParser.GetValidationRules(scenario);
        LocalGithubRepository localRepository = githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name);

        logger.LogDebug("Validate {Repository}", localRepository.GetRepositoryName());
        RepositoryValidationReport report = validationProcessingAction.Process(localRepository, new RepositoryValidationProcessingAction.Request(validationRules));
        reporter.Report(report);
    }
}