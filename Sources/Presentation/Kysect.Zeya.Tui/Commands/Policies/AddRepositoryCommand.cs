using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tui.Controls;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Commands.Policies;

public class AddRepositoryCommand(
    IValidationPolicyRepositoryApi validationPolicyRepositoryApi,
    PolicySelectorControl policySelectorControl) : ITuiCommand
{
    public void Execute()
    {
        ValidationPolicyDto? policy = policySelectorControl.SelectPolicy();
        if (policy is null)
            return;

        string repositoryOwner = AnsiConsole.Ask<string>("Enter repository owner");
        string repositoryName = AnsiConsole.Ask<string>("Enter repository name");
        validationPolicyRepositoryApi.AddRepository(policy.Id, repositoryOwner, repositoryName);
    }
}