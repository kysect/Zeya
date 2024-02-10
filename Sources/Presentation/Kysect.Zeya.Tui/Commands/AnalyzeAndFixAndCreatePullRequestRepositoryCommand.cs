using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration.Abstraction.Contracts;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.Models;
using Kysect.Zeya.Tui.Controls;
using Kysect.Zeya.ValidationRules.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixAndCreatePullRequestRepositoryCommand(
    IGitIntegrationService gitIntegrationService,
    IGithubIntegrationService githubIntegrationService,
    RepositoryValidator repositoryValidator,
    ILogger logger,
    RepositoryDiagnosticFixer repositoryDiagnosticFixer,
    RepositoryValidationRuleProvider validationRuleProvider,
    IGithubRepositoryProvider githubRepositoryProvider)
    : ITuiCommand
{
    public string Name => "Analyze, fix and create PR repository";

    public void Execute()
    {
        // TODO: reduce copy-paste
        GithubRepositoryName githubRepositoryName = RepositoryInputControl.Ask();
        ClonedGithubRepository repository = githubRepositoryProvider.GetGithubRepository(githubRepositoryName.Owner, githubRepositoryName.Name);

        // TODO: remove hardcoded value
        logger.LogTrace("Loading validation configuration");
        IReadOnlyCollection<IValidationRule> rules = validationRuleProvider.GetValidationRules(@"Demo-validation.yaml");
        RepositoryValidationReport report = repositoryValidator.Validate(repository, rules);

        logger.LogInformation("Repositories analyzed, run fixers");
        gitIntegrationService.CreateFixBranch(repository, "zeya/fixer");
        IReadOnlyCollection<IValidationRule> fixedDiagnostics = repositoryDiagnosticFixer.Fix(report, rules, repository);

        logger.LogInformation("Commit fixes");
        gitIntegrationService.CreateCommitWithFix(repository, "Apply Zeya fixes");

        logger.LogInformation("Push changes to remote");
        githubIntegrationService.PushCommitToRemote(repository, "zeya/fixer");

        logger.LogInformation("Create PR");
        var pullRequestMessageCreator = new PullRequestMessageCreator();
        string pullRequestMessage = pullRequestMessageCreator.Create(fixedDiagnostics);
        githubIntegrationService.CreatePullRequest(githubRepositoryName, pullRequestMessage);
    }
}