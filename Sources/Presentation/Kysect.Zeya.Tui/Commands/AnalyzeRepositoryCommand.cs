using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tui.Controls;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeRepositoryCommand : ITuiCommand
{
    private readonly IGitIntegrationService _gitIntegrationService;
    private readonly RepositoryValidator _repositoryValidator;
    private readonly IRepositoryValidationReporter _reporter;
    private readonly IClonedRepositoryFactory<ClonedGithubRepositoryAccessor> _clonedRepositoryFactory;

    public AnalyzeRepositoryCommand(IGitIntegrationService gitIntegrationService, RepositoryValidator repositoryValidator, IRepositoryValidationReporter reporter, IClonedRepositoryFactory<ClonedGithubRepositoryAccessor> clonedRepositoryFactory)
    {
        _gitIntegrationService = gitIntegrationService;
        _repositoryValidator = repositoryValidator;
        _reporter = reporter;
        _clonedRepositoryFactory = clonedRepositoryFactory;
    }

    public string Name => "Analyze repository";

    public void Execute()
    {
        GithubRepository githubRepository = RepositoryInputControl.Ask();
        _gitIntegrationService.CloneOrUpdate(githubRepository);
        ClonedGithubRepositoryAccessor githubRepositoryAccessor = _clonedRepositoryFactory.Create(githubRepository);
        // TODO: remove hardcoded value
        RepositoryValidationReport report = _repositoryValidator.Validate(githubRepositoryAccessor, @"Demo-validation.yaml");
        _reporter.Report(report);
    }
}