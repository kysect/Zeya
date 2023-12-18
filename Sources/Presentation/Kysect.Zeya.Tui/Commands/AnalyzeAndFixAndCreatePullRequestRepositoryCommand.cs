using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.ValidationRules.Fixers;
using Kysect.Zeya.ValidationRules.Rules;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixAndCreatePullRequestRepositoryCommand : ITuiCommand
{
    private readonly IGithubIntegrationService _githubIntegrationService;
    private readonly RepositoryValidator _repositoryValidator;
    private readonly IValidationRuleFixerApplier _validationRuleFixerApplier;
    private readonly ILocalStoragePathFactory _pathFormatStrategy;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public AnalyzeAndFixAndCreatePullRequestRepositoryCommand(IGithubIntegrationService githubIntegrationService, RepositoryValidator repositoryValidator, IValidationRuleFixerApplier validationRuleFixerApplier, ILogger logger, IFileSystem fileSystem, ILocalStoragePathFactory pathFormatStrategy)
    {
        _githubIntegrationService = githubIntegrationService;
        _repositoryValidator = repositoryValidator;
        _validationRuleFixerApplier = validationRuleFixerApplier;
        _logger = logger;
        _fileSystem = fileSystem;
        _pathFormatStrategy = pathFormatStrategy;
    }

    public string Name => "Analyze, fix and create PR repository";

    public void Execute()
    {
        // TODO: reduce copy-paste
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
        _githubIntegrationService.CreateFixBranch(githubRepository);
        List<IValidationRule> fixedDiagnostics = new List<IValidationRule>();

        foreach (var grouping in report.Diagnostics.GroupBy(d => d.Code))
        {
            RepositoryValidationDiagnostic diagnostic = grouping.First();
            // TODO: rework this hack
            IValidationRule validationRule = rules.First(r => r.DiagnosticCode == diagnostic.Code);

            if (_validationRuleFixerApplier.IsFixerRegistered(validationRule))
            {
                _logger.LogInformation("Apply code fixer for {Code}", diagnostic.Code);
                _validationRuleFixerApplier.Apply(validationRule, githubRepositoryAccessor);
                fixedDiagnostics.Add(validationRule);
            }
            else
            {
                _logger.LogDebug("Fixer for {Code} is not available", diagnostic.Code);
            }
        }

        _logger.LogInformation("Commit fixes");
        _githubIntegrationService.CreateCommitWithFix(githubRepository);
        _logger.LogInformation("Push changes to remote");
        _githubIntegrationService.PushCommitToRemote(githubRepository);

        _logger.LogInformation("Create PR");
        var pullRequestMessageCreator = new PullRequestMessageCreator();
        string pullRequestMessage = pullRequestMessageCreator.Create(fixedDiagnostics);
        _githubIntegrationService.CreatePullRequest(githubRepositoryAccessor, pullRequestMessage);
    }
}