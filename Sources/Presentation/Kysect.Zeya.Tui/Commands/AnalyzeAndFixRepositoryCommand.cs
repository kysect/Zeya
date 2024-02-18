using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.Tui.Controls;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixRepositoryCommand(
    RepositorySelectorControl repositorySelector,
    RepositoryValidationService repositoryValidationService,
    RepositoryNameInputControl repositoryNameInputControl)
    : ITuiCommand
{
    public string Name => "Analyze and fix repository";

    public void Execute()
    {
        GithubRepositoryName githubRepositoryName = repositoryNameInputControl.Ask();
        LocalGithubRepository repository = repositorySelector.CreateGithubRepository(githubRepositoryName);
        // TODO: remove hardcoded value
        repositoryValidationService.AnalyzerAndFix(repository, "Demo-validation.yaml");
    }
}