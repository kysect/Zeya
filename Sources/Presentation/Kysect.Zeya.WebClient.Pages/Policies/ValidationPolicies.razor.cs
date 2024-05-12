using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Kysect.Zeya.WebClient.Pages.Policies;

public partial class ValidationPolicies
{
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private IPolicyService PolicyService { get; set; } = null!;

    private GridItemsProvider<ValidationPolicyDto> _policyProvider = null!;

    protected override Task OnInitializedAsync()
    {
        _policyProvider = GridItemsProvider;
        return Task.CompletedTask;
    }

    private void EditPolicy(Guid id)
    {
        _navigationManager.NavigateTo($"validation-policies/{id}");
    }

    public async ValueTask<GridItemsProviderResult<ValidationPolicyDto>> GridItemsProvider(GridItemsProviderRequest<ValidationPolicyDto> request)
    {
        IReadOnlyCollection<ValidationPolicyDto> policies = await PolicyService.GetPolicies();
        return GridItemsProviderResult.From(policies.ToList(), policies.Count);
    }
}