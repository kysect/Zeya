using Kysect.GithubUtils.RepositorySync;
using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.ValidationRules.Fixers;
using Kysect.Zeya.ValidationRules.Rules;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixRepositoryCommand : ITuiCommand
{
    private readonly IGithubIntegrationService _githubIntegrationService;
    private readonly RepositoryValidator _repositoryValidator;
    private readonly IValidationRuleFixerApplier _validationRuleFixerApplier;
    private readonly IPathFormatStrategy _pathFormatStrategy;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public AnalyzeAndFixRepositoryCommand(IGithubIntegrationService githubIntegrationService, RepositoryValidator repositoryValidator, IValidationRuleFixerApplier validationRuleFixerApplier, ILogger logger, IFileSystem fileSystem, IPathFormatStrategy pathFormatStrategy)
    {
        _githubIntegrationService = githubIntegrationService;
        _repositoryValidator = repositoryValidator;
        _validationRuleFixerApplier = validationRuleFixerApplier;
        _logger = logger;
        _fileSystem = fileSystem;
        _pathFormatStrategy = pathFormatStrategy;
    }

    public string Name => "Analyze and fix repository";

    public void Execute()
    {
        var repositoryFullName = AnsiConsole.Ask<string>("Repository (format: org/repo):");
        if (!repositoryFullName.Contains('/'))
            throw new ArgumentException("Incorrect repository format");

        var parts = repositoryFullName.Split('/', 2);
        var githubRepository = new GithubRepository(parts[0], parts[1]);
        var githubRepositoryAccessor = new GithubRepositoryAccessor(githubRepository, _pathFormatStrategy, _fileSystem);
        _githubIntegrationService.CloneOrUpdate(githubRepository);

        // TODO: remove hardcoded value
        var rules = _repositoryValidator.GetValidationRules(@"Demo-validation.yaml");
        var report = _repositoryValidator.Validate(githubRepository, rules);

        _logger.LogInformation("Repositories analyzed, run fixers");
        foreach (var grouping in report.Diagnostics.GroupBy(d => d.Code))
        {
            var diagnostic = grouping.First();
            // TODO: rework this hack
            IValidationRule validationRule = rules.First(r => r.DiagnosticCode == diagnostic.Code);

            if (_validationRuleFixerApplier.IsFixerRegistered(validationRule))
            {
                _logger.LogInformation("Apply code fixer for {Code}", diagnostic.Code);
                _validationRuleFixerApplier.Apply(validationRule, githubRepositoryAccessor);
            }
            else
            {
                _logger.LogDebug("Fixer for {Code} is not available", diagnostic.Code);
            }
        }
    }
}