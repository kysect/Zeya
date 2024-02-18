using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.IntegrationManager;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Controls;

public class PolicySelectorControl(ValidationPolicyService validationPolicyService, IAnsiConsole ansiConsole)
{
    public ValidationPolicyEntity? SelectPolicy()
    {
        IReadOnlyCollection<ValidationPolicyEntity> policies = validationPolicyService.ReadPolicies();

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