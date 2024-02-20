using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Client.Abstractions.Models;
using Kysect.Zeya.Tui.Controls;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Commands;

public class AddRepositoryCommand(
    IValidationPolicyApi validationPolicyApi,
    PolicySelectorControl policySelectorControl) : ITuiCommand
{
    public void Execute()
    {
        ValidationPolicyDto? policy = policySelectorControl.SelectPolicy();
        if (policy is null)
            return;

        string repositoryOwner = AnsiConsole.Ask<string>("Enter repository owner");
        string repositoryName = AnsiConsole.Ask<string>("Enter repository name");
        validationPolicyApi.AddRepository(policy.Id, repositoryOwner, repositoryName);
    }
}