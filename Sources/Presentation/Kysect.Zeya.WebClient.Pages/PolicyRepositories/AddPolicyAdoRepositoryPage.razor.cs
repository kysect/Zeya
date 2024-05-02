using Kysect.Zeya.Client.Abstractions;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Kysect.Zeya.WebClient.Pages.PolicyRepositories;

public partial class AddPolicyAdoRepositoryPage
{
    public class AddPolicyAdoRepositoryPageForm
    {
        [Required]
        public string RemoteHttpsUrl { get; set; } = string.Empty;
        public string? SolutionPathMask { get; set; }
    }

    [Parameter] public Guid PolicyId { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IPolicyRepositoryService PolicyRepositoryService { get; set; } = null!;

    private readonly AddPolicyAdoRepositoryPageForm _formData = new AddPolicyAdoRepositoryPageForm();

    private async Task AddPolicyRepository()
    {
        await PolicyRepositoryService.AddAdoRepository(PolicyId, _formData.RemoteHttpsUrl, _formData.SolutionPathMask);
        NavigationManager.NavigateTo($"/validation-policies/{PolicyId}");
    }
}