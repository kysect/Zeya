using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tui.Controls;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeRepositoriesCommand(
    IPolicyRepositoryValidationService policyRepositoryValidationService,
    RepositoryNameInputControl repositoryNameInputControl) : ITuiCommand
{
    public void Execute()
    {
        GithubRepositoryNameDto githubRepositoryName = repositoryNameInputControl.AskDto();
        // TODO: remove hardcoded value
        policyRepositoryValidationService.Analyze(githubRepositoryName, "Demo-validation.yaml");
    }
}