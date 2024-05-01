using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.CreatePullRequest;

public class RepositoryCreatePullRequestProcessingAction(
    IGitIntegrationService gitIntegrationService,
    IGithubIntegrationService githubIntegrationService,
    PullRequestMessageCreator pullRequestMessageCreator,
    ILogger<RepositoryCreatePullRequestProcessingAction> logger) : IRepositoryProcessingAction<RepositoryCreatePullRequestProcessingAction.Request, RepositoryCreatePullRequestProcessingAction.Response>
{
    public record Request(IReadOnlyCollection<IValidationRule> Rules, IReadOnlyCollection<string> ValidationRuleCodeForFix, IReadOnlyCollection<IValidationRule> FixedDiagnostics);
    public record Response();

    public RepositoryProcessingResponse<Response> Process(ILocalRepository repository, Request request)
    {
        repository.ThrowIfNull();
        request.ThrowIfNull();

        // TODO:
        if (repository is not IClonedLocalRepository clonedLocalRepository)
            throw new ArgumentException("Repository should be cloned");

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
        githubIntegrationService.PushCommitToRemote(repository.FileSystem.GetFullPath(), branchName);

        logger.LogInformation("Create PR");
        string pullRequestMessage = pullRequestMessageCreator.Create(request.FixedDiagnostics);
        clonedLocalRepository.CreatePullRequest(pullRequestMessage, pullRequestTitle, branchName, baseBranch).Wait();
        return new RepositoryProcessingResponse<Response>("Create Pull Request", new Response(), []);
    }
}