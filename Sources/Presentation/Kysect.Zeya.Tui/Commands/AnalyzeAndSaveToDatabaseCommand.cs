using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tui.Controls;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeAndSaveToDatabaseCommand(
    IPolicyValidationService policyValidationService,
    PolicySelectorControl policySelectorControl) : ITuiCommand
{
    public void Execute()
    {
        ValidationPolicyDto? policy = policySelectorControl.SelectPolicy();
        if (policy is null)
            return;

        policyValidationService.Validate(policy.Id);
    }
}