using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Application;

// TODO: merge with PolicyRepositoryValidationService
public class RepositoryValidationService(
    ValidationRuleParser validationRuleParser,
    RepositoryValidator repositoryValidator,
    IRepositoryValidationReporter reporter,
    RepositoryDiagnosticFixer repositoryDiagnosticFixer,
    IGitIntegrationService gitIntegrationService,
    IGithubIntegrationService githubIntegrationService,
    PullRequestMessageCreator pullRequestMessageCreator,
    ILogger<RepositoryValidationService> logger)
{
    public void AnalyzerAndFix(ILocalRepository repository, string scenarioContent)
    {
        IReadOnlyCollection<IValidationRule> validationRules = validationRuleParser.GetValidationRules(scenarioContent);
        RepositoryValidationReport repositoryValidationReport = Analyze([repository], scenarioContent);
        Fix(repository, validationRules, repositoryValidationReport.GetAllDiagnosticRuleCodes());
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
        IReadOnlyCollection<IValidationRule> fixedDiagnostics = Fix(repository, rules, validationRuleCodeForFix);

        logger.LogInformation("Commit fixes");
        gitIntegrationService.CreateCommitWithFix(repository.FileSystem.GetFullPath(), commitMessage);

        logger.LogInformation("Push changes to remote");
        githubIntegrationService.PushCommitToRemote(repository.FileSystem.GetFullPath(), branchName);

        logger.LogInformation("Create PR");
        string pullRequestMessage = pullRequestMessageCreator.Create(fixedDiagnostics);
        await repository.CreatePullRequest(pullRequestMessage, pullRequestTitle, branchName, baseBranch);
    }

    public RepositoryValidationReport Analyze(IReadOnlyCollection<ILocalRepository> repositories, string scenarioContent)
    {
        repositories.ThrowIfNull();

        logger.LogInformation("Start repositories validation");
        RepositoryValidationReport report = RepositoryValidationReport.Empty;
        foreach (ILocalRepository githubRepository in repositories)
        {
            report = report.Compose(AnalyzeSingleRepository(githubRepository, scenarioContent));
        }

        return report;
    }

    public IReadOnlyCollection<IValidationRule> Fix(ILocalRepository repository, IReadOnlyCollection<IValidationRule> validationRules, IReadOnlyCollection<string> validationRuleCodeForFix)
    {
        repository.ThrowIfNull();
        validationRules.ThrowIfNull();

        // TODO: log fix result
        logger.LogInformation("Run fixer for {Repository}", repository.GetRepositoryName());
        return repositoryDiagnosticFixer.Fix(validationRules, repository, validationRuleCodeForFix);
    }

    public RepositoryValidationReport AnalyzeSingleRepository(ILocalRepository repository, string scenarioContent)
    {
        repository.ThrowIfNull();

        IReadOnlyCollection<IValidationRule> validationRules = validationRuleParser.GetValidationRules(scenarioContent);

        logger.LogDebug("Validate {Repository}", repository.GetRepositoryName());
        RepositoryValidationReport report = repositoryValidator.Validate(repository, validationRules);

        reporter.Report(report);
        return report;
    }
}