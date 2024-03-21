using Kysect.ScenarioLib.Abstractions;
using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tui.Controls;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixAndCreatePullRequestRepositoryCommand(
    IScenarioContentProvider scenarioProvider,
    IPolicyRepositoryValidationService policyRepositoryValidationService,
    RepositoryNameInputControl repositoryNameInputControl)
    : ITuiCommand
{
    public void Execute()
    {
        GithubRepositoryNameDto githubRepositoryName = repositoryNameInputControl.AskDto();
        // TODO: remove hardcoded value
        string scenarioSourceCode = scenarioProvider.GetScenarioSourceCode("Demo-validation.yaml");
        policyRepositoryValidationService.CreatePullRequestWithFix(githubRepositoryName, scenarioSourceCode);
    }
}