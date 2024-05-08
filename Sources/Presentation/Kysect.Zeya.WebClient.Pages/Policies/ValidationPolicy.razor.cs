using Kysect.Zeya.Client.Abstractions;
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
    [Inject] private IRepositoryDependenciesService RepositoryDependenciesService { get; set; } = null!;

    private ValidationPolicyDto? _policy;
    private IReadOnlyCollection<ValidationPolicyRepositoryDto> _repositories = new List<ValidationPolicyRepositoryDto>();
    private IReadOnlyCollection<RepositoryDiagnosticTableRow> _diagnosticsTable = new List<RepositoryDiagnosticTableRow>();
    private string? _changesPreview;
    private string? _dependencyTree;

    private GridItemsProvider<ValidationPolicyRepositoryDto> _repositoryProvider = null!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _policy = await PolicyService.GetPolicy(PolicyId);
        _repositories = await PolicyRepositoryService.GetRepositories(PolicyId);
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
        StateHasChanged();
    }

    public async Task CreateFix(Guid repositoryId)
    {
        await PolicyValidationService.CreateFix(PolicyId, repositoryId);
    }

    public async Task PreviewChanges(Guid repositoryId)
    {
        _changesPreview = await PolicyValidationService.PreviewChanges(PolicyId, repositoryId);
    }

    public void NavigateToDetailPage(Guid repositoryId)
    {
        _navigationManager.NavigateTo($"/validation-policies/{PolicyId}/repository/{repositoryId}");
    }

    public async Task LoadDependencyTree()
    {
        _dependencyTree = await RepositoryDependenciesService.GetRepositoryDependenciesTree(PolicyId);
        StateHasChanged();
    }

    public ValueTask<GridItemsProviderResult<ValidationPolicyRepositoryDto>> GridItemsProvider(GridItemsProviderRequest<ValidationPolicyRepositoryDto> request)
    {
        IReadOnlyCollection<ValidationPolicyRepositoryDto> repositories = _repositories;
        return ValueTask.FromResult(GridItemsProviderResult.From(repositories.ToList(), repositories.Count));
    }
}