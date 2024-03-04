using Kysect.Zeya.Dtos;

namespace Kysect.Zeya.Client.Abstractions.Contracts;

public interface IRepositoryValidationApi
{
    void CreatePullRequestWithFix(GithubRepositoryNameDto repository, string scenario);
    void AnalyzerAndFix(GithubRepositoryNameDto repository, string scenario);
    void Analyze(GithubRepositoryNameDto repository, string scenario);
}