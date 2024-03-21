using Kysect.Zeya.Dtos;

namespace Kysect.Zeya.Client.Abstractions;

public interface IPolicyRepositoryValidationService
{
    Task CreatePullRequestWithFix(GithubRepositoryNameDto repository, string scenario);
    void AnalyzerAndFix(GithubRepositoryNameDto repository, string scenario);
    void Analyze(GithubRepositoryNameDto repository, string scenario);
}