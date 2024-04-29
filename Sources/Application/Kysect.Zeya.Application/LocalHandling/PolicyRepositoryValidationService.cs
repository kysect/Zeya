using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.CreatePullRequest;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Fix;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Application.LocalHandling;

public class PolicyRepositoryValidationService(
    ValidationRuleParser validationRuleParser,
    RepositoryValidationProcessingAction validationProcessingAction,
    IRepositoryValidationReporter reporter,
    LocalRepositoryProvider localRepositoryProvider,
    RepositoryFixProcessingAction repositoryDiagnosticFixer,
    RepositoryCreatePullRequestProcessingAction createPullRequestProcessingAction,
    ILogger<PolicyRepositoryValidationService> logger) : IPolicyRepositoryValidationService
{
    public Task CreatePullRequestWithFix(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        IReadOnlyCollection<IValidationRule> validationRules = validationRuleParser.GetValidationRules(scenario);
        LocalGithubRepository localGithubRepository = localRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name);
        RepositoryValidationReport repositoryValidationReport = validationProcessingAction.Process(localGithubRepository, new RepositoryValidationProcessingAction.Request(validationRules));
        reporter.Report(repositoryValidationReport);
        IReadOnlyCollection<string> validationRuleCodeForFix = repositoryValidationReport.GetAllDiagnosticRuleCodes();


        logger.LogInformation("Repositories analyzed, run fixers");
        IReadOnlyCollection<IValidationRule> rules = validationRuleParser.GetValidationRules(scenario);
        IReadOnlyCollection<IValidationRule> fixedDiagnostics = repositoryDiagnosticFixer.Process(localGithubRepository, new RepositoryFixProcessingAction.Request(rules, validationRuleCodeForFix)).FixedRules;
        createPullRequestProcessingAction.Process(localGithubRepository, new RepositoryCreatePullRequestProcessingAction.Request(rules, validationRuleCodeForFix, fixedDiagnostics));

        return Task.CompletedTask;
    }

    public void AnalyzerAndFix(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        LocalGithubRepository localGithubRepository = localRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name);
        IReadOnlyCollection<IValidationRule> validationRules = validationRuleParser.GetValidationRules(scenario);
        RepositoryValidationReport repositoryValidationReport = validationProcessingAction.Process(localGithubRepository, new RepositoryValidationProcessingAction.Request(validationRules));
        reporter.Report(repositoryValidationReport);
        repositoryDiagnosticFixer.Process(localGithubRepository, new RepositoryFixProcessingAction.Request(validationRules, repositoryValidationReport.GetAllDiagnosticRuleCodes()));
    }

    public void Analyze(GithubRepositoryNameDto repository, string scenario)
    {
        logger.LogTrace("Loading validation configuration");
        IReadOnlyCollection<IValidationRule> validationRules = validationRuleParser.GetValidationRules(scenario);
        LocalGithubRepository localRepository = localRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name);

        logger.LogDebug("Validate {Repository}", localRepository.GetRepositoryName());
        RepositoryValidationReport report = validationProcessingAction.Process(localRepository, new RepositoryValidationProcessingAction.Request(validationRules));
        reporter.Report(report);
    }
}