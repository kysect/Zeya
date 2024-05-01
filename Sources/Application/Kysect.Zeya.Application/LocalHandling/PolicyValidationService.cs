using Kysect.Zeya.Application.DatabaseQueries;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.CreatePullRequest;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Fix;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Application.LocalHandling;

public class PolicyValidationService(
    ValidationRuleParser validationRuleParser,
    RepositoryValidationProcessingAction validationProcessingAction,
    RepositoryCreatePullRequestProcessingAction createPullRequestProcessingAction,
    ValidationPolicyDatabaseQueries databaseQueries,
    LocalRepositoryProvider localRepositoryProvider,
    ValidationPolicyRepositoryFactory repositoryFactory,
    RepositoryFixProcessingAction repositoryDiagnosticFixer,
    IGitIntegrationService gitIntegrationService,
    ZeyaDbContext context,
    ILogger<PolicyValidationService> logger) : IPolicyValidationService
{
    public async Task Validate(Guid policyId)
    {
        ValidationPolicyEntity policy = await databaseQueries.GetPolicy(policyId);

        IReadOnlyCollection<ValidationPolicyRepository> repositories = await context
            .ValidationPolicyRepositories
            .Where(r => r.ValidationPolicyId == policyId)
            .ToListAsync();

        IReadOnlyCollection<IValidationRule> validationRules = validationRuleParser.GetValidationRules(policy.Content);

        foreach (ValidationPolicyRepository validationPolicyRepository in repositories)
        {
            IValidationPolicyRepository repository = repositoryFactory.Create(validationPolicyRepository);
            ILocalRepository localGithubRepository = localRepositoryProvider.InitializeRepository(repository);

            var response = validationProcessingAction.Process(localGithubRepository, new RepositoryValidationProcessingActionRequest(validationRules));
            await databaseQueries.SaveValidationResults(validationPolicyRepository.Id, response.Messages);
            await databaseQueries.SaveProcessingActionResult(validationPolicyRepository.Id, response);
        }
    }

    public async Task CreateFix(Guid policyId, Guid repositoryId)
    {
        ValidationPolicyEntity policy = await databaseQueries.GetPolicy(policyId);
        ValidationPolicyRepository repositoryInfo = await context
            .ValidationPolicyRepositories
            .Where(r => r.ValidationPolicyId == policyId)
            .Where(r => r.Id == repositoryId)
            .SingleAsync();

        IValidationPolicyRepository repository = repositoryFactory.Create(repositoryInfo);
        ILocalRepository localGithubRepository = localRepositoryProvider.InitializeRepository(repository);

        List<string> validationRuleIds = await context
            .ValidationPolicyRepositoryDiagnostics
            .Where(d => d.ValidationPolicyRepositoryId == repositoryId)
            .Select(d => d.RuleId)
            .ToListAsync();

        logger.LogInformation("Repositories analyzed, run fixers");
        IReadOnlyCollection<IValidationRule> rules = validationRuleParser.GetValidationRules(policy.Content);
        var fixResponse = repositoryDiagnosticFixer.Process(localGithubRepository, new RepositoryFixProcessingActionRequest(rules, validationRuleIds));
        await databaseQueries.SaveProcessingActionResult(repositoryInfo.Id, fixResponse);

        IReadOnlyCollection<IValidationRule> fixedDiagnostics = fixResponse.Value.FixedRules;
        var createPullRequestResponse = createPullRequestProcessingAction.Process(localGithubRepository, new RepositoryCreatePullRequestProcessingActionRequest(rules, validationRuleIds, fixedDiagnostics));
        await databaseQueries.SaveProcessingActionResult(repositoryInfo.Id, createPullRequestResponse);
    }

    public async Task<string> PreviewChanges(Guid policyId, Guid repositoryId)
    {
        ValidationPolicyEntity policy = await databaseQueries.GetPolicy(policyId);
        ValidationPolicyRepository repositoryInfo = await context
            .ValidationPolicyRepositories
            .Where(r => r.ValidationPolicyId == policyId)
            .Where(r => r.Id == repositoryId)
            .SingleAsync();

        IValidationPolicyRepository repository = repositoryFactory.Create(repositoryInfo);
        ILocalRepository localGithubRepository = localRepositoryProvider.InitializeRepository(repository);
        List<string> validationRuleIds = await context
            .ValidationPolicyRepositoryDiagnostics
            .Where(d => d.ValidationPolicyRepositoryId == repositoryId)
            .Select(d => d.RuleId)
            .ToListAsync();

        IReadOnlyCollection<IValidationRule> rules = validationRuleParser.GetValidationRules(policy.Content);
        var response = repositoryDiagnosticFixer.Process(localGithubRepository, new RepositoryFixProcessingActionRequest(rules, validationRuleIds));
        await databaseQueries.SaveProcessingActionResult(repositoryInfo.Id, response);
        return gitIntegrationService.GetDiff(localGithubRepository.FileSystem.GetFullPath());
    }
}