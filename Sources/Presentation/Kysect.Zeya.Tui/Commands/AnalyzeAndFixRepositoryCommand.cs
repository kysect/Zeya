using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.ValidationRules.Fixers;
using Kysect.Zeya.ValidationRules.Rules;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixRepositoryCommand(
    IGithubIntegrationService githubIntegrationService,
    RepositoryValidator repositoryValidator,
    IValidationRuleFixerApplier validationRuleFixerApplier,
    ILogger logger,
    GithubRepositoryAccessorFactory githubRepositoryAccessorFactory)
    : ITuiCommand
{
    public string Name => "Analyze and fix repository";

    public void Execute()
    {
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
        foreach (var grouping in report.Diagnostics.GroupBy(d => d.Code))
        {
            var diagnostic = grouping.First();
            // TODO: rework this hack
            IValidationRule validationRule = rules.First(r => r.DiagnosticCode == diagnostic.Code);

            if (validationRuleFixerApplier.IsFixerRegistered(validationRule))
            {
                logger.LogInformation("Apply code fixer for {Code}", diagnostic.Code);
                validationRuleFixerApplier.Apply(validationRule, githubRepositoryAccessor);
            }
            else
            {
                logger.LogDebug("Fixer for {Code} is not available", diagnostic.Code);
            }
        }
    }
}