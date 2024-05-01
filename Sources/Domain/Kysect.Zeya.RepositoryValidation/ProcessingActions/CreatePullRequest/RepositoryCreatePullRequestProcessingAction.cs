﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.CreatePullRequest;

public record RepositoryCreatePullRequestProcessingActionRequest(IReadOnlyCollection<IValidationRule> Rules, IReadOnlyCollection<string> ValidationRuleCodeForFix, IReadOnlyCollection<IValidationRule> FixedDiagnostics);
public record RepositoryCreatePullRequestProcessingActionResponse();

public class RepositoryCreatePullRequestProcessingAction(
    IGitIntegrationService gitIntegrationService,
    PullRequestMessageCreator pullRequestMessageCreator,
    GitRepositoryCredentialOptions gitRepositoryCredentialOptions,
    ILogger<RepositoryCreatePullRequestProcessingAction> logger)
    : IRepositoryProcessingAction<RepositoryCreatePullRequestProcessingActionRequest, RepositoryCreatePullRequestProcessingActionResponse>
{
    public RepositoryProcessingResponse<RepositoryCreatePullRequestProcessingActionResponse> Process(ILocalRepository repository, RepositoryCreatePullRequestProcessingActionRequest request)
    {
        repository.ThrowIfNull();
        request.ThrowIfNull();

        // TODO:
        if (repository is not LocalGithubRepository clonedLocalRepository)
            throw new ArgumentException("Repository should be LocalGithubRepository");

        // TODO: handle that branch already exists
        // TODO: remove hardcoded value
        // TODO: support case when base branch is not master
        string baseBranch = "master";
        string branchName = "zeya/fixer";
        string pullRequestTitle = "Fix warnings from Zeya";
        string commitMessage = "Apply Zeya code fixers";


        gitIntegrationService.CreateFixBranch(repository.FileSystem.GetFullPath(), branchName);

        logger.LogInformation("Commit fixes");
        gitIntegrationService.CreateCommitWithFix(repository.FileSystem.GetFullPath(), commitMessage);

        logger.LogInformation("Push changes to remote");
        gitIntegrationService.PushCommitToRemote(repository.FileSystem.GetFullPath(), branchName, gitRepositoryCredentialOptions);

        logger.LogInformation("Create PR");
        string pullRequestMessage = pullRequestMessageCreator.Create(request.FixedDiagnostics);
        clonedLocalRepository.GitHubIntegrationService.CreatePullRequest(clonedLocalRepository.GithubMetadata, pullRequestMessage, pullRequestTitle, branchName, baseBranch);

        return new RepositoryProcessingResponse<RepositoryCreatePullRequestProcessingActionResponse>("Create Pull Request", new RepositoryCreatePullRequestProcessingActionResponse(), []);
    }
}