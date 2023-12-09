using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Spectre.Console;

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
        var repositoryFullName = AnsiConsole.Ask<string>("Repository (format: org/repo):");
        if (!repositoryFullName.Contains('/'))
            throw new ArgumentException("Incorrect repository format");

        var parts = repositoryFullName.Split('/', 2);
        var githubRepository = new GithubRepository(parts[0], parts[1]);

        _githubIntegrationService.CloneOrUpdate(githubRepository);
        // TODO: remove hardcoded value
        var report = _repositoryValidator.Validate(githubRepository, @"Demo-validation.yaml");

        _reporter.Report(report);
    }
}