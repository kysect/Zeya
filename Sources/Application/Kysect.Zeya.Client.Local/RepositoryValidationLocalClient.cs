using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Client.Abstractions.Models;
using Kysect.Zeya.IntegrationManager;

namespace Kysect.Zeya.Client.Local;

public class RepositoryValidationLocalClient(
    IGithubRepositoryProvider githubRepositoryProvider,
    RepositoryValidationService repositoryValidationService) : IRepositoryValidationApi
{
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