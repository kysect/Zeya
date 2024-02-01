using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tui.Controls;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixAndCreatePullRequestRepositoryCommand(
    IGithubIntegrationService githubIntegrationService,
    RepositoryValidator repositoryValidator,
    ILogger logger,
    IClonedRepositoryFactory clonedRepositoryFactory,
    RepositoryDiagnosticFixer repositoryDiagnosticFixer)
    : ITuiCommand
{
    public string Name => "Analyze, fix and create PR repository";

    public void Execute()
    {
        // TODO: reduce copy-paste
        GithubRepository githubRepository = RepositoryInputControl.Ask();
        githubIntegrationService.CloneOrUpdate(githubRepository);

        // TODO: remove hardcoded value
        IReadOnlyCollection<IValidationRule> rules = repositoryValidator.GetValidationRules(@"Demo-validation.yaml");
        RepositoryValidationReport report = repositoryValidator.Validate(githubRepository, rules);

        logger.LogInformation("Repositories analyzed, run fixers");
        githubIntegrationService.CreateFixBranch(githubRepository, "zeya/fixer");
        IClonedRepository githubRepositoryAccessor = clonedRepositoryFactory.Create(githubRepository);
        IReadOnlyCollection<IValidationRule> fixedDiagnostics = repositoryDiagnosticFixer.Fix(report, rules, githubRepositoryAccessor);

        logger.LogInformation("Commit fixes");
        githubIntegrationService.CreateCommitWithFix(githubRepository, "Apply Zeya fixes");

        logger.LogInformation("Push changes to remote");
        githubIntegrationService.PushCommitToRemote(githubRepository, "zeya/fixer");

        logger.LogInformation("Create PR");
        var pullRequestMessageCreator = new PullRequestMessageCreator();
        string pullRequestMessage = pullRequestMessageCreator.Create(fixedDiagnostics);
        githubIntegrationService.CreatePullRequest(githubRepository, pullRequestMessage);
    }
}