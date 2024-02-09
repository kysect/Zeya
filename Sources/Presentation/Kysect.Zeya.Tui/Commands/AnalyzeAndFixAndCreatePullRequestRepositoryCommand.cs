using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration.Abstraction.Contracts;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Kysect.Zeya.RepositoryValidation;
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
        GithubRepository githubRepository = RepositoryInputControl.Ask();
        IClonedRepository repository = githubRepositoryProvider.GetGithubRepository(githubRepository.Owner, githubRepository.Name);
        ClonedGithubRepositoryAccessor githubRepositoryAccessor = repository.To<ClonedGithubRepositoryAccessor>();

        // TODO: remove hardcoded value
        logger.LogTrace("Loading validation configuration");
        IReadOnlyCollection<IValidationRule> rules = validationRuleProvider.GetValidationRules(@"Demo-validation.yaml");
        RepositoryValidationReport report = repositoryValidator.Validate(githubRepositoryAccessor, rules);

        logger.LogInformation("Repositories analyzed, run fixers");
        gitIntegrationService.CreateFixBranch(githubRepositoryAccessor, "zeya/fixer");
        IReadOnlyCollection<IValidationRule> fixedDiagnostics = repositoryDiagnosticFixer.Fix(report, rules, githubRepositoryAccessor);

        logger.LogInformation("Commit fixes");
        gitIntegrationService.CreateCommitWithFix(githubRepositoryAccessor, "Apply Zeya fixes");

        logger.LogInformation("Push changes to remote");
        gitIntegrationService.PushCommitToRemote(githubRepositoryAccessor, "zeya/fixer");

        logger.LogInformation("Create PR");
        var pullRequestMessageCreator = new PullRequestMessageCreator();
        string pullRequestMessage = pullRequestMessageCreator.Create(fixedDiagnostics);
        githubIntegrationService.CreatePullRequest(githubRepository, pullRequestMessage);
    }
}