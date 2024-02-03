using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzerGithubOrganization : ITuiCommand
{
    private readonly IGitIntegrationService _gitIntegrationService;
    private readonly IRepositoryValidationReporter _reporter;
    private readonly IGithubRepositoryProvider _githubRepositoryProvider;
    private readonly RepositoryValidator _repositoryValidator;
    private readonly IClonedRepositoryFactory<ClonedGithubRepositoryAccessor> _clonedRepositoryFactory;
    private readonly RepositoryValidationRuleProvider _validationRuleProvider;
    private readonly ILogger _logger;

    public AnalyzerGithubOrganization(IGitIntegrationService gitIntegrationService, IRepositoryValidationReporter reporter, IGithubRepositoryProvider githubRepositoryProvider, RepositoryValidator repositoryValidator, IClonedRepositoryFactory<ClonedGithubRepositoryAccessor> clonedRepositoryFactory, RepositoryValidationRuleProvider validationRuleProvider, ILogger logger)
    {
        _gitIntegrationService = gitIntegrationService;
        _reporter = reporter;
        _githubRepositoryProvider = githubRepositoryProvider;
        _repositoryValidator = repositoryValidator;
        _clonedRepositoryFactory = clonedRepositoryFactory;
        _validationRuleProvider = validationRuleProvider;
        _logger = logger;
    }

    public string Name => "Analyze Kysect Github organization";

    public void Execute()
    {
        _logger.LogInformation("Start Zeya demo");
        _logger.LogInformation("Loading github repositories for validation");
        IReadOnlyCollection<GithubRepository> repositories = _githubRepositoryProvider.GetAll();

        _logger.LogInformation("Get {Count} repositories for cloning", repositories.Count);
        foreach (GithubRepository repository in repositories)
            _gitIntegrationService.CloneOrUpdate(repository);

        _logger.LogTrace("Loading validation configuration");
        IReadOnlyCollection<IValidationRule> validationRules = _validationRuleProvider.GetValidationRules("Demo-validation.yaml");

        var report = RepositoryValidationReport.Empty;
        _logger.LogInformation("Start repositories validation");
        foreach (GithubRepository githubRepository in repositories)
        {
            ClonedGithubRepositoryAccessor clonedGithubRepositoryAccessor = _clonedRepositoryFactory.Create(githubRepository);
            report = report.Compose(_repositoryValidator.Validate(clonedGithubRepositoryAccessor, validationRules));
        }

        _reporter.Report(report);
    }
}