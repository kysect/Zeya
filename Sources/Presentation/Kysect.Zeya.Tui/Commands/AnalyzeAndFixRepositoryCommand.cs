using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration.Abstraction.Contracts;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tui.Controls;
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
        IClonedRepository repository = githubRepositoryProvider.GetGithubRepository(githubRepository.Owner, githubRepository.Name);
        ClonedGithubRepositoryAccessor githubRepositoryAccessor = repository.To<ClonedGithubRepositoryAccessor>();

        // TODO: remove hardcoded value
        logger.LogTrace("Loading validation configuration");
        IReadOnlyCollection<IValidationRule> rules = validationRuleProvider.GetValidationRules(@"Demo-validation.yaml");
        RepositoryValidationReport report = repositoryValidator.Validate(githubRepositoryAccessor, rules);

        logger.LogInformation("Repositories analyzed, run fixers");
        IReadOnlyCollection<IValidationRule> fixedRules = repositoryDiagnosticFixer.Fix(report, rules, githubRepositoryAccessor);
    }
}