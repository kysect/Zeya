using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tui.Controls;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndSaveToDatabaseCommand(
    RepositoryValidationService repositoryValidationService,
    IGithubRepositoryProvider githubRepositoryProvider,
    ValidationPolicyService validationPolicyService) : ITuiCommand
{
    public string Name => "Analyze and save to database";

    public void Execute()
    {
        var policySelectorControl = new PolicySelectorControl(validationPolicyService);
        ValidationPolicyEntity? policy = policySelectorControl.SelectPolicy();
        if (policy is null)
            return;

        IReadOnlyCollection<ValidationPolicyRepository> repositories = validationPolicyService.GetRepositories(policy.Id);
        foreach (ValidationPolicyRepository validationPolicyRepository in repositories)
        {
            LocalGithubRepository localGithubRepository = githubRepositoryProvider.GetGithubRepository(validationPolicyRepository.GithubOwner, validationPolicyRepository.GithubRepository);
            RepositoryValidationReport report = repositoryValidationService.AnalyzeSingleRepository(localGithubRepository, "Demo-validation.yaml");
            validationPolicyService.SaveReport(validationPolicyRepository, report);
        }
    }
}