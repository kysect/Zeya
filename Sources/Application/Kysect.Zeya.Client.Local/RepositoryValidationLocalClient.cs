using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Client.Abstractions.Models;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;

namespace Kysect.Zeya.Client.Local;

public class RepositoryValidationLocalClient(
    ValidationPolicyService service,
    IGithubRepositoryProvider githubRepositoryProvider,
    RepositoryValidationService repositoryValidationService) : IRepositoryValidationApi
{
    public void ValidatePolicyRepositories(Guid policyId)
    {
        ValidationPolicyEntity policy = service.GetPolicy(policyId).Result;

        IReadOnlyCollection<ValidationPolicyRepository> repositories = service.GetRepositories(policy.Id).Result;
        foreach (ValidationPolicyRepository validationPolicyRepository in repositories)
        {
            LocalGithubRepository localGithubRepository = githubRepositoryProvider.GetGithubRepository(validationPolicyRepository.GithubOwner, validationPolicyRepository.GithubRepository);
            // TODO: looks like bug, we must use content from policy instead of this hardcoded value
            RepositoryValidationReport report = repositoryValidationService.AnalyzeSingleRepository(localGithubRepository, "Demo-validation.yaml");
            service.SaveReport(validationPolicyRepository, report).Wait();
        }
    }

    public void CreatePullRequestWithFix(GithubRepositoryNameDto repository, string scenario)
    {
        repositoryValidationService.CreatePullRequestWithFix(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenario);
    }

    public void AnalyzerAndFix(GithubRepositoryNameDto repository, string scenario)
    {
        repositoryValidationService.AnalyzerAndFix(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenario);
    }

    public void Analyze(GithubRepositoryNameDto repository, string scenario)
    {
        repositoryValidationService.AnalyzeSingleRepository(githubRepositoryProvider.GetGithubRepository(repository.Owner, repository.Name), scenario);
    }
}