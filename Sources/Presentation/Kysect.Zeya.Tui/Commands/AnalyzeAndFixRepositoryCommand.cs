using System.IO.Abstractions;
using Kysect.GithubUtils.RepositorySync;
using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.ValidationRules.Fixers;
using Microsoft.Extensions.Logging;
using Spectre.Console;

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
        var report = _repositoryValidator.Validate(githubRepository, @"Demo-validation.yaml");

        foreach (var diagnostic in report.Diagnostics)
        {
            if (_validationRuleFixerApplier.IsFixerRegistered(diagnostic.Code))
            {
                _logger.LogInformation("Apply code fixer for {Code} {Message}", diagnostic.Code, diagnostic.Message);
                _validationRuleFixerApplier.Apply(diagnostic.Code, githubRepositoryAccessor);
            }
        }
    }
}