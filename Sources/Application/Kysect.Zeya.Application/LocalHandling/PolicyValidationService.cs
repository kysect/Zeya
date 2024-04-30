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
    ValidationPolicyService service,
    LocalRepositoryProvider localRepositoryProvider,
    ValidationPolicyRepositoryFactory repositoryFactory,
    RepositoryFixProcessingAction repositoryDiagnosticFixer,
    IGitIntegrationService gitIntegrationService,
    ZeyaDbContext context,
    ILogger<PolicyValidationService> logger) : IPolicyValidationService
{
    public async Task Validate(Guid policyId)
    {
        ValidationPolicyEntity policy = await service.GetPolicy(policyId);

        IReadOnlyCollection<ValidationPolicyRepository> repositories = await context
            .ValidationPolicyRepositories
            .Where(r => r.ValidationPolicyId == policyId)
            .ToListAsync();

        IReadOnlyCollection<IValidationRule> validationRules = validationRuleParser.GetValidationRules(policy.Content);

        foreach (ValidationPolicyRepository validationPolicyRepository in repositories)
        {
            IValidationPolicyRepository repository = repositoryFactory.Create(validationPolicyRepository);
            ILocalRepository localGithubRepository = localRepositoryProvider.InitializeRepository(repository);

            RepositoryValidationReport report = validationProcessingAction.Process(localGithubRepository, new RepositoryValidationProcessingAction.Request(validationRules));

            await service.SaveReport(validationPolicyRepository.Id, report);
            await service.SaveValidationActionMessages(validationPolicyRepository.Id, report);
        }
    }

    public async Task CreateFix(Guid policyId, Guid repositoryId)
    {
        ValidationPolicyEntity policy = await service.GetPolicy(policyId);
        ValidationPolicyRepository repositoryInfo = await context
            .ValidationPolicyRepositories
            .Where(r => r.ValidationPolicyId == policyId)
            .Where(r => r.Id == repositoryId)
            .SingleAsync();

        IValidationPolicyRepository repository = repositoryFactory.Create(repositoryInfo);
        ILocalRepository localGithubRepository = localRepositoryProvider.InitializeRepository(repository);

        if (localGithubRepository is not IClonedLocalRepository clonedLocalRepository)
            throw new NotSupportedException($"Repository {localGithubRepository.GetType()} does not have remote. Cannot create PR.");

        List<string> validationRuleIds = await context
            .ValidationPolicyRepositoryDiagnostics
            .Where(d => d.ValidationPolicyRepositoryId == repositoryId)
            .Select(d => d.RuleId)
            .ToListAsync();

        logger.LogInformation("Repositories analyzed, run fixers");
        IReadOnlyCollection<IValidationRule> rules = validationRuleParser.GetValidationRules(policy.Content);
        IReadOnlyCollection<IValidationRule> fixedDiagnostics = repositoryDiagnosticFixer.Process(clonedLocalRepository, new RepositoryFixProcessingAction.Request(rules, validationRuleIds)).FixedRules;
        createPullRequestProcessingAction.Process(clonedLocalRepository, new RepositoryCreatePullRequestProcessingAction.Request(rules, validationRuleIds, fixedDiagnostics));
    }

    public async Task<string> PreviewChanges(Guid policyId, Guid repositoryId)
    {
        ValidationPolicyEntity policy = await service.GetPolicy(policyId);
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
        repositoryDiagnosticFixer.Process(localGithubRepository, new RepositoryFixProcessingAction.Request(rules, validationRuleIds));
        return gitIntegrationService.GetDiff(localGithubRepository.FileSystem.GetFullPath());
    }
}