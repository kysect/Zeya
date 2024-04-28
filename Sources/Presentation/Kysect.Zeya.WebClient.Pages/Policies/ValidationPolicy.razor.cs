﻿using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kysect.Zeya.WebClient.Pages.Policies;

public partial class ValidationPolicy
{
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;

    [Parameter] public Guid PolicyId { get; set; }

    [Inject] private IPolicyService PolicyService { get; set; } = null!;
    [Inject] private IPolicyRepositoryService PolicyRepositoryService { get; set; } = null!;
    [Inject] private IPolicyValidationService PolicyValidationService { get; set; } = null!;

    private ValidationPolicyDto? _policy;
    private IReadOnlyCollection<RepositoryDiagnosticTableRow> _diagnosticsTable = new List<RepositoryDiagnosticTableRow>();
    private string? _changesPreview;

    private GridItemsProvider<ValidationPolicyRepositoryDto> _repositoryProvider = null!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _policy = await PolicyService.GetPolicy(PolicyId);
        _diagnosticsTable = await PolicyService.GetDiagnosticsTable(PolicyId);
        _repositoryProvider = GridItemsProvider;
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

    public async Task CreateFix(Guid repositoryId)
    {
        await PolicyValidationService.CreateFix(PolicyId, repositoryId);
    }

    public async Task PreviewChanges(Guid repositoryId)
    {
        _changesPreview = await PolicyValidationService.PreviewChanges(PolicyId, repositoryId);
    }

    public async ValueTask<GridItemsProviderResult<ValidationPolicyRepositoryDto>> GridItemsProvider(GridItemsProviderRequest<ValidationPolicyRepositoryDto> request)
    {
        var repositories = await PolicyRepositoryService.GetRepositories(PolicyId);
        return GridItemsProviderResult.From(repositories.ToList(), repositories.Count);
    }
}