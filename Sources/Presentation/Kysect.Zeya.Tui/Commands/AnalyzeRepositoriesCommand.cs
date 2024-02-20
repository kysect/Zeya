using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Client.Abstractions.Models;
using Kysect.Zeya.Tui.Controls;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeRepositoriesCommand(
    IRepositoryValidationApi repositoryValidationApi,
    RepositoryNameInputControl repositoryNameInputControl) : ITuiCommand
{
    public void Execute()
    {
        GithubRepositoryNameDto githubRepositoryName = repositoryNameInputControl.AskDto();
        // TODO: remove hardcoded value
        repositoryValidationApi.Analyze(githubRepositoryName, "Demo-validation.yaml");
    }
}