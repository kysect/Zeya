using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IGithubIntegrationService
{
    void CreatePullRequest(GithubRepository repository, string message);
    bool DeleteBranchOnMerge(GithubRepository githubRepository);
    RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepository githubRepository, string branch);
}