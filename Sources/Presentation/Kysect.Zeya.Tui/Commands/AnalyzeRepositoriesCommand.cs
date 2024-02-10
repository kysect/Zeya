using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.Abstractions;
using Kysect.Zeya.RepositoryValidation.Abstractions.Models;
using Kysect.Zeya.Tui.Controls;
using Kysect.Zeya.ValidationRules.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeRepositoriesCommand(
    IRepositoryValidationReporter reporter,
    IGithubRepositoryProvider githubRepositoryProvider,
    RepositoryValidator repositoryValidator,
    RepositoryValidationRuleProvider validationRuleProvider,
    ILogger logger) : ITuiCommand
{
    public string Name => "Analyze repositories";

    public void Execute()
    {
        var repositorySelector = new RepositorySelector(githubRepositoryProvider);

        IReadOnlyCollection<IClonedRepository> repositories = repositorySelector.SelectRepositories();

        logger.LogTrace("Loading validation configuration");
        // TODO: remove hardcoded value
        IReadOnlyCollection<IValidationRule> validationRules = validationRuleProvider.GetValidationRules("Demo-validation.yaml");

        var report = RepositoryValidationReport.Empty;
        logger.LogInformation("Start repositories validation");
        foreach (IClonedRepository githubRepository in repositories)
        {
            logger.LogDebug("Validate {Repository}", githubRepository.GetFullPath());
            report = report.Compose(repositoryValidator.Validate(githubRepository, validationRules));
        }

        reporter.Report(report);
    }
}