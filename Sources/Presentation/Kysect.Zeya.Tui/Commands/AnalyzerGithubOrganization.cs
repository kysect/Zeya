using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration.Abstraction.Contracts;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.RepositoryValidation;
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
        IReadOnlyCollection<IClonedRepository> organizationRepositories = githubRepositoryProvider.GetGithubOrganizationRepositories(organization);

        logger.LogTrace("Loading validation configuration");
        IReadOnlyCollection<IValidationRule> validationRules = validationRuleProvider.GetValidationRules("Demo-validation.yaml");

        var report = RepositoryValidationReport.Empty;
        logger.LogInformation("Start repositories validation");
        foreach (IClonedRepository githubRepository in organizationRepositories)
        {
            ClonedGithubRepositoryAccessor clonedGithubRepositoryAccessor = githubRepository.To<ClonedGithubRepositoryAccessor>();
            report = report.Compose(repositoryValidator.Validate(clonedGithubRepositoryAccessor, validationRules));
        }

        reporter.Report(report);
    }
}