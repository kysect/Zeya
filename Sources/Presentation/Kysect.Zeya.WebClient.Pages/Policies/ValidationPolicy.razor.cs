using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Dtos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kysect.Zeya.WebClient.Pages.Policies;

public partial class ValidationPolicy
{
    [Parameter] public Guid PolicyId { get; set; }

    [Inject] private IValidationPolicyApi _validationPolicyApi { get; set; } = null!;
    [Inject] private IValidationPolicyRepositoryApi _policyRepositoryApi { get; set; } = null!;
    [Inject] private IPolicyValidationApi _policyValidationApi { get; set; } = null!;

    private ValidationPolicyDto? _policy;
    private IReadOnlyCollection<ValidationPolicyRepositoryDto> _policyRepositories = new List<ValidationPolicyRepositoryDto>();
    private IReadOnlyCollection<RepositoryDiagnosticTableRow> _diagnosticsTable = new List<RepositoryDiagnosticTableRow>();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _policy = await _validationPolicyApi.GetPolicy(PolicyId);
        _policyRepositories = await _policyRepositoryApi.GetRepositories(PolicyId);
        _diagnosticsTable = await _validationPolicyApi.GetDiagnosticsTable(PolicyId);
    }

    public IReadOnlyCollection<string> GetDiagnosticHeaders()
    {
        return _diagnosticsTable
            .SelectMany(d => d.Diagnostics.Keys)
            .Distinct()
            .ToList();
    }

    public async Task RunValidation()
    {
        await _policyValidationApi.Validate(PolicyId);
    }

    public async Task CreateFix(string repositoryOwner, string repositoryName)
    {
        await _policyValidationApi.CreateFix(PolicyId, repositoryOwner, repositoryName);
    }
}