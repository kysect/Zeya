using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Client.Abstractions.Models;
using Kysect.Zeya.Tui.Controls;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndSaveToDatabaseCommand(
    IPolicyValidationApi policyValidationApi,
    PolicySelectorControl policySelectorControl) : ITuiCommand
{
    public void Execute()
    {
        ValidationPolicyDto? policy = policySelectorControl.SelectPolicy();
        if (policy is null)
            return;

        policyValidationApi.Validate(policy.Id);
    }
}