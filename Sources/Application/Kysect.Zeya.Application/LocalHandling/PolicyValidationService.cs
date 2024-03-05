using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using Microsoft.EntityFrameworkCore;

namespace Kysect.Zeya.Application.LocalHandling;

public class PolicyValidationService(
    ValidationPolicyService service,
    IGithubRepositoryProvider githubRepositoryProvider,
    RepositoryValidationService policyRepositoryValidationService,
    ValidationPolicyRepositoryFactory repositoryFactory,
    ZeyaDbContext context) : IPolicyValidationService
{
    public async Task Validate(Guid policyId)
    {
        ValidationPolicyEntity policy = await service.GetPolicy(policyId);
        var scenarioContent = new ScenarioContent(policy.Content);

        IReadOnlyCollection<ValidationPolicyRepository> repositories = await context
            .ValidationPolicyRepositories
            .Where(r => r.ValidationPolicyId == policyId)
            .ToListAsync();

        foreach (ValidationPolicyRepository validationPolicyRepository in repositories)
        {
            IValidationPolicyRepository repository = repositoryFactory.Create(validationPolicyRepository);
            ILocalRepository localGithubRepository = githubRepositoryProvider.InitializeRepository(repository);
            RepositoryValidationReport report = policyRepositoryValidationService.AnalyzeSingleRepository(localGithubRepository, scenarioContent);
            await service.SaveReport(validationPolicyRepository.Id, report);
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
        ILocalRepository localGithubRepository = githubRepositoryProvider.InitializeRepository(repository);

        // TODO:
        if (localGithubRepository is not LocalGithubRepository githubRepository)
            throw new NotImplementedException($"Repository with type {localGithubRepository} cannot be fixed");

        // TODO: issues #89 No need to analyze, we already have report in database
        policyRepositoryValidationService.CreatePullRequestWithFix(githubRepository, new ScenarioContent(policy.Content));
    }
}