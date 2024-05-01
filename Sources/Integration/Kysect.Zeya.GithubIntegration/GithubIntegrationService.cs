using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Kysect.Zeya.GithubIntegration;

public class GithubIntegrationService(
    IGitHubClient gitHubClient,
    ILogger<GithubIntegrationService> logger)
    : IGithubIntegrationService
{
    private readonly IGitHubClient _gitHubClient = gitHubClient.ThrowIfNull();
    private readonly ILogger _logger = logger.ThrowIfNull();

    public async Task CreatePullRequest(GithubRepositoryName repositoryName, string message, string pullRequestTitle, string branch, string baseBranch)
    {
        repositoryName.ThrowIfNull();
        message.ThrowIfNull();

        // TODO: return PR info
        PullRequest pullRequest = await _gitHubClient.Repository.PullRequest.Create(
            repositoryName.Owner,
            repositoryName.Name,
            new NewPullRequest(pullRequestTitle, branch, baseBranch) { Body = message });
    }

    public async Task<bool> DeleteBranchOnMerge(GithubRepositoryName githubRepositoryName)
    {
        githubRepositoryName.ThrowIfNull();

        var repositoryInfo = await _gitHubClient.Repository.Get(githubRepositoryName.Owner, githubRepositoryName.Name);
        return repositoryInfo.DeleteBranchOnMerge ?? false;
    }

    public RepositoryBranchProtection GetRepositoryBranchProtection(GithubRepositoryName githubRepositoryName, string branch)
    {
        githubRepositoryName.ThrowIfNull();

        try
        {
            BranchProtectionSettings repositoryBranchProtection = _gitHubClient.Repository.Branch.GetBranchProtection(githubRepositoryName.Owner, githubRepositoryName.Name, branch).Result;
            return new RepositoryBranchProtection(repositoryBranchProtection.RequiredPullRequestReviews is not null, repositoryBranchProtection.RequiredConversationResolution?.Enabled ?? false);
        }
        catch (Exception e)
        {
            // TODO: rework this. Possible exception: NotFound, Forbidden (for private repo)
            _logger.LogWarning("Failed to get branch protection info for {Repository}: {Message}", githubRepositoryName.FullName, e.Message);
            return new RepositoryBranchProtection(false, false);
        }
    }
}