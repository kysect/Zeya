using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;

namespace Kysect.Zeya.Client.Local;

public class PolicyValidationLocalClient(
    ValidationPolicyService service,
    IGithubRepositoryProvider githubRepositoryProvider,
    RepositoryValidationService repositoryValidationService) : IPolicyValidationApi
{
    public async Task Validate(Guid policyId)
    {
        ValidationPolicyEntity policy = await service.GetPolicy(policyId);
        ScenarioContent scenarioContent = new ScenarioContent(policy.Content);

        IReadOnlyCollection<ValidationPolicyRepository> repositories = await service.GetRepositories(policy.Id);
        foreach (ValidationPolicyRepository validationPolicyRepository in repositories)
        {
            LocalGithubRepository localGithubRepository = githubRepositoryProvider.GetGithubRepository(validationPolicyRepository.GithubOwner, validationPolicyRepository.GithubRepository);
            // TODO: looks like bug, we must use content from policy instead of this hardcoded value
            RepositoryValidationReport report = repositoryValidationService.AnalyzeSingleRepository(localGithubRepository, scenarioContent);
            await service.SaveReport(validationPolicyRepository, report);
        }
    }
}