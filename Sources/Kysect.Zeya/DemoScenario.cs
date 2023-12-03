using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya;

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
        _logger.LogInformation("Loading github repositories for validation");
        IReadOnlyCollection<GithubRepository> repositories = _githubRepositoryProvider.GetAll();

        _logger.LogInformation("Clone {Count} repositories", repositories.Count);
        foreach (var repository in repositories)
        {
            _logger.LogDebug($"Clone repository {repository.FullName}");
            _githubIntegrationService.CloneOrUpdate(repository);
        }

        var report = RepositoryValidationReport.Empty;
        foreach (var githubRepository in repositories)
            report = report.Compose(_repositoryValidator.Validate(githubRepository, "Demo-validation"));

        _reporter.Report(report);
    }
}