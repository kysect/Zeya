using Kysect.CommonLib.Logging;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya;

public class DemoScenario
{
    private readonly IGithubIntegrationService _githubIntegrationService;
    private readonly IRepositoryValidationReporter _reporter;
    private readonly IGithubRepositoryProvider _githubRepositoryProvider;
    private readonly ILogger _logger;

    public DemoScenario(
        IGithubIntegrationService githubIntegrationService,
        IRepositoryValidationReporter reporter,
        IGithubRepositoryProvider githubRepositoryProvider,
        ILogger logger)
    {
        _githubIntegrationService = githubIntegrationService;
        _reporter = reporter;
        _logger = logger;
        _githubRepositoryProvider = githubRepositoryProvider;
    }

    public void Process(
        IReadOnlyCollection<IRepositoryValidationRule<GithubRepository>> validationRules)
    {
        _logger.LogInformation("Loading github repositories for validation");
        IReadOnlyCollection<GithubRepository> repositories = _githubRepositoryProvider.GetAll();

        _logger.LogInformation("Clone {Count} repositories", repositories.Count);
        foreach (var repository in repositories)
        {
            _logger.LogTabDebug(1, $"Clone repository {repository.FullName}");
            _githubIntegrationService.CloneOrUpdate(repository);
        }

        var report = RepositoryValidationReport.Empty;
        var repositoryValidator = new RepositoryValidator(_logger);
        foreach (var githubRepository in repositories)
            report = report.Compose(repositoryValidator.Validate(githubRepository, validationRules));

        _reporter.Report(report);
    }
}