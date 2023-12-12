using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.ValidationRules.Rules;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Demo;

public class DemoScenario
{
    private readonly IGithubIntegrationService _githubIntegrationService;   
    private readonly IRepositoryValidationReporter _reporter;
    private readonly IGithubRepositoryProvider _githubRepositoryProvider;
    private readonly RepositoryValidator _repositoryValidator;
    private readonly ILogger _logger;

    public DemoScenario(
        IGithubIntegrationService githubIntegrationService,
        IRepositoryValidationReporter reporter,
        IGithubRepositoryProvider githubRepositoryProvider,
        ILogger logger,
        RepositoryValidator repositoryValidator)
    {
        _githubIntegrationService = githubIntegrationService;
        _reporter = reporter;
        _logger = logger;
        _repositoryValidator = repositoryValidator;
        _githubRepositoryProvider = githubRepositoryProvider;
    }

    public void Process()
    {
        _logger.LogInformation("Start Zeya demo");
        _logger.LogInformation("Loading github repositories for validation");
        IReadOnlyCollection<GithubRepository> repositories = _githubRepositoryProvider.GetAll();

        _logger.LogInformation("Get {Count} repositories for cloning", repositories.Count);
        foreach (var repository in repositories)
            _githubIntegrationService.CloneOrUpdate(repository);

        IReadOnlyCollection<IValidationRule> validationRules = _repositoryValidator.GetValidationRules("Demo-validation.yaml");

        var report = RepositoryValidationReport.Empty;
        _logger.LogInformation("Start repositories validation");
        foreach (var githubRepository in repositories)
            report = report.Compose(_repositoryValidator.Validate(githubRepository, validationRules));

        _reporter.Report(report);
    }
}