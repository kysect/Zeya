using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Client.Abstractions.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kysect.Zeya.WebClient.Pages.Policies;

public partial class ValidationPolicies
{
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private IValidationPolicyApi _validationPolicyApi { get; set; } = null!;

    private IReadOnlyCollection<ValidationPolicyDto>? _validationPolicies;

    protected override async Task OnInitializedAsync()
    {
        _validationPolicies = await _validationPolicyApi.GetPolicies();
    }

    private void EditPolicy(Guid id)
    {
        _navigationManager.NavigateTo($"validation-policies/{id}");
    }
}