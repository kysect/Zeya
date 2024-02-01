using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tui.Controls;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeRepositoryCommand : ITuiCommand
{
    private readonly IGithubIntegrationService _githubIntegrationService;
    private readonly RepositoryValidator _repositoryValidator;
    private readonly IRepositoryValidationReporter _reporter;

    public AnalyzeRepositoryCommand(IGithubIntegrationService githubIntegrationService, RepositoryValidator repositoryValidator, IRepositoryValidationReporter reporter)
    {
        _githubIntegrationService = githubIntegrationService;
        _repositoryValidator = repositoryValidator;
        _reporter = reporter;
    }

    public string Name => "Analyze repository";

    public void Execute()
    {
        GithubRepository githubRepository = RepositoryInputControl.Ask();
        _githubIntegrationService.CloneOrUpdate(githubRepository);
        // TODO: remove hardcoded value
        RepositoryValidationReport report = _repositoryValidator.Validate(githubRepository, @"Demo-validation.yaml");
        _reporter.Report(report);
    }
}