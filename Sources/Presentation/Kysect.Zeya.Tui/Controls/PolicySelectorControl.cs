using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Controls;

public class PolicySelectorControl(IPolicyService policyService, IAnsiConsole ansiConsole)
{
    public ValidationPolicyDto? SelectPolicy()
    {
        IReadOnlyCollection<ValidationPolicyDto> policies = policyService.GetPolicies().Result;

        if (policies.Count == 0)
        {
            ansiConsole.WriteLine("No policies found.");
            return null;
        }

        string[] policyNames = policies.Select(p => p.Name).ToArray();
        string selectedPolicyName = ansiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select policy")
                .AddChoices(policyNames));

        return policies.First(p => p.Name == selectedPolicyName);
    }
}