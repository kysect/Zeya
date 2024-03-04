using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;

namespace Kysect.Zeya.Application.LocalHandling;

public class PolicyValidationService(
    ValidationPolicyService service,
    IGithubRepositoryProvider githubRepositoryProvider,
    RepositoryValidationService policyRepositoryValidationService) : IPolicyValidationService
{
    public async Task Validate(Guid policyId)
    {
        ValidationPolicyEntity policy = await service.GetPolicy(policyId);
        var scenarioContent = new ScenarioContent(policy.Content);

        IReadOnlyCollection<ValidationPolicyRepository> repositories = await service.GetRepositories(policy.Id);
        foreach (ValidationPolicyRepository validationPolicyRepository in repositories)
        {
            LocalGithubRepository localGithubRepository = githubRepositoryProvider.GetGithubRepository(validationPolicyRepository.GithubOwner, validationPolicyRepository.GithubRepository);
            RepositoryValidationReport report = policyRepositoryValidationService.AnalyzeSingleRepository(localGithubRepository, scenarioContent);
            await service.SaveReport(validationPolicyRepository, report);
        }
    }

    public async Task CreateFix(Guid policyId, string repositoryOwner, string repositoryName)
    {
        ValidationPolicyEntity policy = await service.GetPolicy(policyId);
        ValidationPolicyRepository repository = await service.GetRepository(policyId, repositoryOwner, repositoryName);
        LocalGithubRepository localGithubRepository = githubRepositoryProvider.GetGithubRepository(repository.GithubOwner, repository.GithubRepository);

        // TODO: issues #89 No need to analyze, we already have report in database
        policyRepositoryValidationService.CreatePullRequestWithFix(localGithubRepository, new ScenarioContent(policy.Content));
    }
}