using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Client.Abstractions.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kysect.Zeya.WebClient.Pages.Policies;

public partial class ValidationPolicy
{
    [Parameter] public Guid PolicyId { get; set; }

    [Inject] private IValidationPolicyApi _validationPolicyApi { get; set; } = null!;
    [Inject] private IValidationPolicyRepositoryApi _policyRepositoryApi { get; set; } = null!;

    private ValidationPolicyDto? _policy;
    private IReadOnlyCollection<ValidationPolicyRepositoryDto> _policyRepositories = new List<ValidationPolicyRepositoryDto>();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _policy = await _validationPolicyApi.GetPolicy(PolicyId);
        _policyRepositories = await _policyRepositoryApi.GetRepositories(PolicyId);
    }
}