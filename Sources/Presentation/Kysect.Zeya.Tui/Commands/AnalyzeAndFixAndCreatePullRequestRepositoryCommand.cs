using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tui.Controls;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixAndCreatePullRequestRepositoryCommand(
    IRepositoryValidationApi repositoryValidationApi,
    RepositoryNameInputControl repositoryNameInputControl)
    : ITuiCommand
{
    public void Execute()
    {
        GithubRepositoryNameDto githubRepositoryName = repositoryNameInputControl.AskDto();
        // TODO: remove hardcoded value
        repositoryValidationApi.CreatePullRequestWithFix(githubRepositoryName, "Demo-validation.yaml");
    }
}