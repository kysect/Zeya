using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.Tui.Controls;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Commands;

public class AddRepositoryCommand(
    ValidationPolicyService validationPolicyService,
    PolicySelectorControl policySelectorControl) : ITuiCommand
{
    public string Name => "Add repository";

    public void Execute()
    {
        ValidationPolicyEntity? policy = policySelectorControl.SelectPolicy();
        if (policy is null)
            return;

        string repositoryOwner = AnsiConsole.Ask<string>("Enter repository owner");
        string repositoryName = AnsiConsole.Ask<string>("Enter repository name");
        validationPolicyService.AddRepository(policy.Id, repositoryOwner, repositoryName);
    }
}