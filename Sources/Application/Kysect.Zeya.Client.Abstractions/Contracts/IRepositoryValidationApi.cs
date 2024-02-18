using Kysect.Zeya.Client.Abstractions.Models;

namespace Kysect.Zeya.Client.Abstractions.Contracts;

public interface IRepositoryValidationApi
{
    void ValidatePolicyRepositories(Guid policyId);
    void CreatePullRequestWithFix(GithubRepositoryNameDto repository, string scenario);
    void AnalyzerAndFix(GithubRepositoryNameDto repository, string scenario);
    void Analyze(GithubRepositoryNameDto repository, string scenario);
}