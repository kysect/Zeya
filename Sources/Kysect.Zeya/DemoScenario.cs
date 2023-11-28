using Kysect.CommonLib.Logging;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya;

public class DemoScenario
{
    private readonly IGithubIntegrationService _githubIntegrationService;
    private readonly IRepositoryValidationReporter _reporter;
    private readonly ILogger _logger;

    public DemoScenario(
        IGithubIntegrationService githubIntegrationService,
        IRepositoryValidationReporter reporter,
        ILogger logger)
    {
        _githubIntegrationService = githubIntegrationService;
        _reporter = reporter;
        _logger = logger;
    }

    public void Process(
        IReadOnlyCollection<GithubRepository> repositories,
        IReadOnlyCollection<IRepositoryValidationRule<GithubRepository>> validationRules)
    {
        var repositoryValidator = new RepositoryValidator(_logger);
        _logger.LogInformation("Clone {Count} repositories", repositories.Count);

        foreach (var repository in repositories)
        {
            _logger.LogTabDebug(1, $"Clone repository {repository.FullName}");
            _githubIntegrationService.CloneOrUpdate(repository);
        }

        RepositoryValidationReport report = RepositoryValidationReport.Empty;
        foreach (var githubRepository in repositories)
            report = report.Compose(repositoryValidator.Validate(githubRepository, validationRules));

        _reporter.Report(report);
    }
}