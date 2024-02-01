using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.ValidationRules.Rules;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixAndCreatePullRequestRepositoryCommand(
    IGithubIntegrationService githubIntegrationService,
    RepositoryValidator repositoryValidator,
    ILogger logger,
    GithubRepositoryAccessorFactory githubRepositoryAccessorFactory,
    RepositoryDiagnosticFixer repositoryDiagnosticFixer)
    : ITuiCommand
{
    public string Name => "Analyze, fix and create PR repository";

    public void Execute()
    {
        // TODO: reduce copy-paste
        var repositoryFullName = AnsiConsole.Ask<string>("Repository (format: org/repo):");
        if (!repositoryFullName.Contains('/'))
            throw new ArgumentException("Incorrect repository format");

        var parts = repositoryFullName.Split('/', 2);
        var githubRepository = new GithubRepository(parts[0], parts[1]);
        ClonedRepository githubRepositoryAccessor = githubRepositoryAccessorFactory.Create(githubRepository);
        githubIntegrationService.CloneOrUpdate(githubRepository);

        // TODO: remove hardcoded value
        IReadOnlyCollection<IValidationRule> rules = repositoryValidator.GetValidationRules(@"Demo-validation.yaml");
        RepositoryValidationReport report = repositoryValidator.Validate(githubRepository, rules);

        logger.LogInformation("Repositories analyzed, run fixers");
        githubIntegrationService.CreateFixBranch(githubRepository, "zeya/fixer");
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