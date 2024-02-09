using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;

namespace Kysect.Zeya.GithubIntegration.Abstraction.Contracts;

public interface IGithubIntegrationService
{
    void CreatePullRequest(GithubRepository repository, string message);
    bool DeleteBranchOnMerge(GithubRepository githubRepository);
    RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepository githubRepository, string branch);
}