using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tui.Controls;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixRepositoryCommand(
    IGithubIntegrationService githubIntegrationService,
    RepositoryValidator repositoryValidator,
    ILogger logger,
    IClonedRepositoryFactory clonedRepositoryFactory,
    RepositoryDiagnosticFixer repositoryDiagnosticFixer)
    : ITuiCommand
{
    public string Name => "Analyze and fix repository";

    public void Execute()
    {
        GithubRepository githubRepository = RepositoryInputControl.Ask();
        githubIntegrationService.CloneOrUpdate(githubRepository);

        // TODO: remove hardcoded value
        IReadOnlyCollection<IValidationRule> rules = repositoryValidator.GetValidationRules(@"Demo-validation.yaml");
        RepositoryValidationReport report = repositoryValidator.Validate(githubRepository, rules);

        logger.LogInformation("Repositories analyzed, run fixers");
        IClonedRepository githubRepositoryAccessor = clonedRepositoryFactory.Create(githubRepository);
        IReadOnlyCollection<IValidationRule> fixedRules = repositoryDiagnosticFixer.Fix(report, rules, githubRepositoryAccessor);
    }
}