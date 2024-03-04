using Kysect.Zeya.Client.Abstractions;
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

    [Inject] private IPolicyService PolicyService { get; set; } = null!;
    [Inject] private IPolicyRepositoryService PolicyRepositoryService { get; set; } = null!;
    [Inject] private IPolicyValidationService PolicyValidationService { get; set; } = null!;

    private ValidationPolicyDto? _policy;
    private IReadOnlyCollection<ValidationPolicyRepositoryDto> _policyRepositories = new List<ValidationPolicyRepositoryDto>();
    private IReadOnlyCollection<RepositoryDiagnosticTableRow> _diagnosticsTable = new List<RepositoryDiagnosticTableRow>();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _policy = await PolicyService.GetPolicy(PolicyId);
        _policyRepositories = await PolicyRepositoryService.GetRepositories(PolicyId);
        _diagnosticsTable = await PolicyService.GetDiagnosticsTable(PolicyId);
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
        await PolicyValidationService.Validate(PolicyId);
    }

    public async Task CreateFix(string repositoryOwner, string repositoryName)
    {
        await PolicyValidationService.CreateFix(PolicyId, repositoryOwner, repositoryName);
    }
}