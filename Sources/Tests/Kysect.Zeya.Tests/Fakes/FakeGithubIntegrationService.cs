using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Tests.Fakes;

public class FakeGithubIntegrationService : IGithubIntegrationService
{
    public RepositoryBranchProtection RepositoryBranchProtection { get; set; }
    public bool BranchProtectionEnabled { get; set; }

    public FakeGithubIntegrationService()
    {
        RepositoryBranchProtection = new RepositoryBranchProtection(false, false);
    }

    public void CloneOrUpdate(GithubRepository repository)
    {
        throw new NotImplementedException();
    }

    public void CreateFixBranch(GithubRepository repository, string branchName)
    {
        throw new NotImplementedException();
    }

    public void CreateCommitWithFix(GithubRepository repository, string commitMessage)
    {
        throw new NotImplementedException();
    }

    public void PushCommitToRemote(GithubRepository repository, string branchName)
    {
        throw new NotImplementedException();
    }

    public void CreatePullRequest(GithubRepository repository, string message)
    {
        throw new NotImplementedException();
    }

    public bool DeleteBranchOnMerge(GithubRepository githubRepository)
    {
        return BranchProtectionEnabled;
    }

    public RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepository githubRepository, string branch)
    {
        return RepositoryBranchProtection;
    }
}