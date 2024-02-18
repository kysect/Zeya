using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.IntegrationManager;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Controls;

public class PolicySelectorControl
{
    private readonly ValidationPolicyService _validationPolicyService;

    public PolicySelectorControl(ValidationPolicyService validationPolicyService)
    {
        _validationPolicyService = validationPolicyService;
    }

    public ValidationPolicyEntity? SelectPolicy()
    {
        IReadOnlyCollection<ValidationPolicyEntity> policies = _validationPolicyService.Read();

        if (policies.Count == 0)
        {
            AnsiConsole.WriteLine("No policies found.");
            return null;
        }

        var policyNames = policies.Select(p => p.Name).ToArray();
        var selectedPolicyName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select policy")
                .PageSize(10)
                .MoreChoicesText("More")
                .AddChoices(policyNames));

        return policies.First(p => p.Name == selectedPolicyName);
    }
}