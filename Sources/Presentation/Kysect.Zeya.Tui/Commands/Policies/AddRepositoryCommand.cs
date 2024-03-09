using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tui.Controls;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Commands.Policies;

public class AddRepositoryCommand(
    IPolicyRepositoryService policyRepositoryService,
    PolicySelectorControl policySelectorControl) : ITuiCommand
{
    public void Execute()
    {
        ValidationPolicyDto? policy = policySelectorControl.SelectPolicy();
        if (policy is null)
            return;

        string repositoryOwner = AnsiConsole.Ask<string>("Enter repository owner");
        string repositoryName = AnsiConsole.Ask<string>("Enter repository name");
        policyRepositoryService.AddGithubRepository(policy.Id, repositoryOwner, repositoryName, null);
    }
}