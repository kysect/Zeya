using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tui.Controls;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeRepositoryCommand : ITuiCommand
{
    private readonly IGitIntegrationService _gitIntegrationService;
    private readonly RepositoryValidator _repositoryValidator;
    private readonly IRepositoryValidationReporter _reporter;
    private readonly IClonedRepositoryFactory<ClonedGithubRepositoryAccessor> _clonedRepositoryFactory;
    private readonly RepositoryValidationRuleProvider _validationRuleProvider;
    private readonly ILogger _logger;

    public AnalyzeRepositoryCommand(IGitIntegrationService gitIntegrationService, RepositoryValidator repositoryValidator, IRepositoryValidationReporter reporter, IClonedRepositoryFactory<ClonedGithubRepositoryAccessor> clonedRepositoryFactory, ILogger logger, RepositoryValidationRuleProvider validationRuleProvider)
    {
        _gitIntegrationService = gitIntegrationService;
        _repositoryValidator = repositoryValidator;
        _reporter = reporter;
        _clonedRepositoryFactory = clonedRepositoryFactory;
        _logger = logger;
        _validationRuleProvider = validationRuleProvider;
    }

    public string Name => "Analyze repository";

    public void Execute()
    {
        GithubRepository githubRepository = RepositoryInputControl.Ask();
        _gitIntegrationService.CloneOrUpdate(githubRepository);
        ClonedGithubRepositoryAccessor githubRepositoryAccessor = _clonedRepositoryFactory.Create(githubRepository);

        _logger.LogInformation("Validate repository {Url}", githubRepositoryAccessor.GithubMetadata.FullName);
        _logger.LogTrace("Loading validation configuration");
        // TODO: remove hardcoded value
        IReadOnlyCollection<IValidationRule> steps = _validationRuleProvider.GetValidationRules(@"Demo-validation.yaml");
        RepositoryValidationReport report = _repositoryValidator.Validate(githubRepositoryAccessor, steps);
        _reporter.Report(report);
    }
}