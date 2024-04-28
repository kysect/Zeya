using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Application;

public class RepositoryValidationService(
    ValidationRuleParser validationRuleParser,
    RepositoryValidationProcessingAction validationProcessingAction,
    IRepositoryValidationReporter reporter,
    RepositoryFixProcessingAction repositoryDiagnosticFixer,
    IGitIntegrationService gitIntegrationService,
    IGithubIntegrationService githubIntegrationService,
    PullRequestMessageCreator pullRequestMessageCreator,
    ILogger<RepositoryValidationService> logger)
{
    public void AnalyzerAndFix(ILocalRepository repository, string scenarioContent)
    {
        IReadOnlyCollection<IValidationRule> validationRules = validationRuleParser.GetValidationRules(scenarioContent);
        RepositoryValidationReport repositoryValidationReport = Analyze([repository], scenarioContent);
        repositoryDiagnosticFixer.Process(repository, new RepositoryFixProcessingAction.Request(validationRules, repositoryValidationReport.GetAllDiagnosticRuleCodes()));
    }

    public async Task CreatePullRequestWithFix(IClonedLocalRepository repository, string scenarioContent, IReadOnlyCollection<string> validationRuleCodeForFix)
    {
        repository.ThrowIfNull();

        // TODO: handle that branch already exists
        // TODO: remove hardcoded value
        // TODO: support case when base branch is not master
        string baseBranch = "master";
        string branchName = "zeya/fixer";
        string pullRequestTitle = "Fix warnings from Zeya";
        string commitMessage = "Apply Zeya code fixers";

        IReadOnlyCollection<IValidationRule> rules = validationRuleParser.GetValidationRules(scenarioContent);

        logger.LogInformation("Repositories analyzed, run fixers");
        gitIntegrationService.CreateFixBranch(repository.FileSystem.GetFullPath(), branchName);
        IReadOnlyCollection<IValidationRule> fixedDiagnostics = repositoryDiagnosticFixer.Process(repository, new RepositoryFixProcessingAction.Request(rules, validationRuleCodeForFix)).FixedRules;

        logger.LogInformation("Commit fixes");
        gitIntegrationService.CreateCommitWithFix(repository.FileSystem.GetFullPath(), commitMessage);

        logger.LogInformation("Push changes to remote");
        githubIntegrationService.PushCommitToRemote(repository.FileSystem.GetFullPath(), branchName);

        logger.LogInformation("Create PR");
        string pullRequestMessage = pullRequestMessageCreator.Create(fixedDiagnostics);
        await repository.CreatePullRequest(pullRequestMessage, pullRequestTitle, branchName, baseBranch);
    }

    public string PreviewChanges(ILocalRepository repository, string scenarioContent, IReadOnlyCollection<string> validationRuleCodeForFix)
    {
        repository.ThrowIfNull();

        IReadOnlyCollection<IValidationRule> rules = validationRuleParser.GetValidationRules(scenarioContent);

        logger.LogInformation("Repositories analyzed, run fixers");
        repositoryDiagnosticFixer.Process(repository, new RepositoryFixProcessingAction.Request(rules, validationRuleCodeForFix));

        return gitIntegrationService.GetDiff(repository.FileSystem.GetFullPath());
    }

    public RepositoryValidationReport Analyze(IReadOnlyCollection<ILocalRepository> repositories, string scenarioContent)
    {
        repositories.ThrowIfNull();

        logger.LogInformation("Start repositories validation");
        RepositoryValidationReport report = RepositoryValidationReport.Empty;
        IReadOnlyCollection<IValidationRule> validationRules = validationRuleParser.GetValidationRules(scenarioContent);

        foreach (ILocalRepository repository in repositories)
        {
            logger.LogDebug("Validate {Repository}", repository.GetRepositoryName());
            RepositoryValidationReport repositoryValidationReport = validationProcessingAction.Process(repository, new RepositoryValidationProcessingAction.Request(validationRules));
            reporter.Report(report);
            report = report.Compose(repositoryValidationReport);
        }

        return report;
    }
}