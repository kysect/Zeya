using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tui.Controls;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndFixRepositoryCommand(
    IPolicyRepositoryValidationService policyRepositoryValidationService,
    RepositoryNameInputControl repositoryNameInputControl)
    : ITuiCommand
{
    public void Execute()
    {
        GithubRepositoryNameDto githubRepositoryName = repositoryNameInputControl.AskDto();
        // TODO: remove hardcoded value
        policyRepositoryValidationService.AnalyzerAndFix(githubRepositoryName, "Demo-validation.yaml");
    }
}