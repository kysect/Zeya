using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.IntegrationManager;

public class RepositoryValidationService(
    RepositoryValidationRuleProvider validationRuleProvider,
    RepositoryValidator repositoryValidator,
    IRepositoryValidationReporter reporter,
    RepositoryDiagnosticFixer repositoryDiagnosticFixer,
    IGitIntegrationService gitIntegrationService,
    IGithubIntegrationService githubIntegrationService,
    PullRequestMessageCreator pullRequestMessageCreator,
    ILogger logger)
{
    public void AnalyzerAndFix(ILocalRepository repository, string scenario)
    {
        IReadOnlyCollection<IValidationRule> validationRules = validationRuleProvider.GetValidationRules(scenario);
        RepositoryValidationReport repositoryValidationReport = Analyze([repository], scenario);
        Fix(repository, repositoryValidationReport, validationRules);
    }

    public void CreatePullRequestWithFix(LocalGithubRepository repository, string scenario)
    {
        repository.ThrowIfNull();

        string branchName = "zeya/fixer";
        string commitMessage = "Apply Zeya code fixers";

        // TODO: remove hardcoded value
        RepositoryValidationReport report = Analyze([repository], scenario);
        IReadOnlyCollection<IValidationRule> rules = validationRuleProvider.GetValidationRules(scenario);

        logger.LogInformation("Repositories analyzed, run fixers");
        gitIntegrationService.CreateFixBranch(repository.FileSystem.GetFullPath(), branchName);
        IReadOnlyCollection<IValidationRule> fixedDiagnostics = Fix(repository, report, rules);

        logger.LogInformation("Commit fixes");
        gitIntegrationService.CreateCommitWithFix(repository.FileSystem.GetFullPath(), commitMessage);

        logger.LogInformation("Push changes to remote");
        githubIntegrationService.PushCommitToRemote(repository.FileSystem.GetFullPath(), branchName);

        logger.LogInformation("Create PR");
        string pullRequestMessage = pullRequestMessageCreator.Create(fixedDiagnostics);
        githubIntegrationService.CreatePullRequest(repository.GithubMetadata, pullRequestMessage);
    }

    public RepositoryValidationReport Analyze(IReadOnlyCollection<ILocalRepository> repositories, string scenario)
    {
        repositories.ThrowIfNull();

        logger.LogTrace("Loading validation configuration");
        IReadOnlyCollection<IValidationRule> validationRules = validationRuleProvider.GetValidationRules(scenario);

        RepositoryValidationReport report = RepositoryValidationReport.Empty;
        logger.LogInformation("Start repositories validation");
        foreach (ILocalRepository githubRepository in repositories)
        {
            logger.LogDebug("Validate {Repository}", githubRepository.GetRepositoryName());
            report = report.Compose(repositoryValidator.Validate(githubRepository, validationRules));
        }

        reporter.Report(report);
        return report;
    }

    public IReadOnlyCollection<IValidationRule> Fix(ILocalRepository repository, RepositoryValidationReport report, IReadOnlyCollection<IValidationRule> validationRules)
    {
        repository.ThrowIfNull();
        report.ThrowIfNull();
        validationRules.ThrowIfNull();

        // TODO: log fix result
        logger.LogInformation("Run fixer for {Repository}", repository.GetRepositoryName());
        return repositoryDiagnosticFixer.Fix(report, validationRules, repository);
    }
}