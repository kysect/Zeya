using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.Models;
using Kysect.Zeya.ValidationRules.Abstractions;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzerGithubOrganization(
    IRepositoryValidationReporter reporter,
    IGithubRepositoryProvider githubRepositoryProvider,
    RepositoryValidator repositoryValidator,
    RepositoryValidationRuleProvider validationRuleProvider,
    ILogger logger)
    : ITuiCommand
{
    public string Name => "Analyze Kysect Github organization";

    public void Execute()
    {
        logger.LogInformation("Start Zeya demo");
        string organization = AnsiConsole.Ask<string>("Organization for clone: ");
        logger.LogInformation("Loading github repositories for validation");
        // TODO: allow to input exclusion
        IReadOnlyCollection<ClonedGithubRepository> organizationRepositories = githubRepositoryProvider.GetGithubOrganizationRepositories(organization, []);

        logger.LogTrace("Loading validation configuration");
        IReadOnlyCollection<IValidationRule> validationRules = validationRuleProvider.GetValidationRules("Demo-validation.yaml");

        var report = RepositoryValidationReport.Empty;
        logger.LogInformation("Start repositories validation");
        foreach (ClonedGithubRepository githubRepository in organizationRepositories)
        {
            report = report.Compose(repositoryValidator.Validate(githubRepository, validationRules));
        }

        reporter.Report(report);
    }
}