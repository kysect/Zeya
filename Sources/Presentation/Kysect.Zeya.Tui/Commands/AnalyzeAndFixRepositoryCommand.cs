using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tui.Controls;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixRepositoryCommand(
    IGitIntegrationService gitIntegrationService,
    RepositoryValidator repositoryValidator,
    ILogger logger,
    IClonedRepositoryFactory<ClonedGithubRepositoryAccessor> clonedRepositoryFactory,
    RepositoryDiagnosticFixer repositoryDiagnosticFixer,
    RepositoryValidationRuleProvider validationRuleProvider)
    : ITuiCommand
{
    public string Name => "Analyze and fix repository";

    public void Execute()
    {
        GithubRepository githubRepository = RepositoryInputControl.Ask();
        gitIntegrationService.CloneOrUpdate(githubRepository);

        // TODO: remove hardcoded value
        logger.LogTrace("Loading validation configuration");
        IReadOnlyCollection<IValidationRule> rules = validationRuleProvider.GetValidationRules(@"Demo-validation.yaml");
        ClonedGithubRepositoryAccessor githubRepositoryAccessor = clonedRepositoryFactory.Create(githubRepository);
        RepositoryValidationReport report = repositoryValidator.Validate(githubRepositoryAccessor, rules);

        logger.LogInformation("Repositories analyzed, run fixers");
        IReadOnlyCollection<IValidationRule> fixedRules = repositoryDiagnosticFixer.Fix(report, rules, githubRepositoryAccessor);
    }
}