using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Microsoft.AspNetCore.Components;

namespace Kysect.Zeya.WebClient.Pages.PolicyRepositories;

public partial class PolicyRepositoryPage
{
    [Parameter] public Guid PolicyId { get; set; }
    [Parameter] public Guid RepositoryId { get; set; }

    [Inject] private IPolicyService PolicyService { get; set; } = null!;
    [Inject] private IPolicyRepositoryService PolicyRepositoryService { get; set; } = null!;

    private ValidationPolicyDto? _policy;
    private ValidationPolicyRepositoryDto? _repository;
    private IReadOnlyCollection<ValidationPolicyRepositoryActionDto>? _actions;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _policy = await PolicyService.GetPolicy(PolicyId);
        _repository = await PolicyRepositoryService.GetRepository(PolicyId, RepositoryId);
        _actions = await PolicyRepositoryService.GetActions(PolicyId, RepositoryId);
    }
}