using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.ValidationRules.Fixers;
using Kysect.Zeya.ValidationRules.Rules;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixAndCreatePullRequestRepositoryCommand(
    IGithubIntegrationService githubIntegrationService,
    RepositoryValidator repositoryValidator,
    IValidationRuleFixerApplier validationRuleFixerApplier,
    ILogger logger,
    GithubRepositoryAccessorFactory githubRepositoryAccessorFactory)
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
        var githubRepositoryAccessor = githubRepositoryAccessorFactory.Create(githubRepository);
        githubIntegrationService.CloneOrUpdate(githubRepository);

        // TODO: remove hardcoded value
        var rules = repositoryValidator.GetValidationRules(@"Demo-validation.yaml");
        var report = repositoryValidator.Validate(githubRepository, rules);

        logger.LogInformation("Repositories analyzed, run fixers");
        githubIntegrationService.CreateFixBranch(githubRepository);
        List<IValidationRule> fixedDiagnostics = new List<IValidationRule>();

        foreach (var grouping in report.Diagnostics.GroupBy(d => d.Code))
        {
            RepositoryValidationDiagnostic diagnostic = grouping.First();
            // TODO: rework this hack
            IValidationRule validationRule = rules.First(r => r.DiagnosticCode == diagnostic.Code);

            if (validationRuleFixerApplier.IsFixerRegistered(validationRule))
            {
                logger.LogInformation("Apply code fixer for {Code}", diagnostic.Code);
                validationRuleFixerApplier.Apply(validationRule, githubRepositoryAccessor);
                fixedDiagnostics.Add(validationRule);
            }
            else
            {
                logger.LogDebug("Fixer for {Code} is not available", diagnostic.Code);
            }
        }

        logger.LogInformation("Commit fixes");
        githubIntegrationService.CreateCommitWithFix(githubRepository);
        logger.LogInformation("Push changes to remote");
        githubIntegrationService.PushCommitToRemote(githubRepository);

        logger.LogInformation("Create PR");
        var pullRequestMessageCreator = new PullRequestMessageCreator();
        string pullRequestMessage = pullRequestMessageCreator.Create(fixedDiagnostics);
        githubIntegrationService.CreatePullRequest(githubRepositoryAccessor, pullRequestMessage);
    }
}