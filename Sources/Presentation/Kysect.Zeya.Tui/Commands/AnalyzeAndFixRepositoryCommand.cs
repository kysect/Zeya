using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.Models;
using Kysect.Zeya.Tui.Controls;
using Kysect.Zeya.ValidationRules.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixRepositoryCommand(
    RepositoryValidator repositoryValidator,
    ILogger logger,
    RepositoryDiagnosticFixer repositoryDiagnosticFixer,
    RepositoryValidationRuleProvider validationRuleProvider,
    IGithubRepositoryProvider githubRepositoryProvider)
    : ITuiCommand
{
    public string Name => "Analyze and fix repository";

    public void Execute()
    {
        GithubRepository githubRepository = RepositoryInputControl.Ask();
        ClonedGithubRepositoryAccessor repository = githubRepositoryProvider.GetGithubRepository(githubRepository.Owner, githubRepository.Name);

        // TODO: remove hardcoded value
        logger.LogTrace("Loading validation configuration");
        IReadOnlyCollection<IValidationRule> rules = validationRuleProvider.GetValidationRules(@"Demo-validation.yaml");
        RepositoryValidationReport report = repositoryValidator.Validate(repository, rules);

        logger.LogInformation("Repositories analyzed, run fixers");
        IReadOnlyCollection<IValidationRule> fixedRules = repositoryDiagnosticFixer.Fix(report, rules, repository);
    }
}