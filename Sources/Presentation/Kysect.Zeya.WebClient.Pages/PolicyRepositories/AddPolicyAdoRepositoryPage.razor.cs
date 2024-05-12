using Kysect.Zeya.Client.Abstractions;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.WebClient.Pages.PolicyRepositories;

public partial class AddPolicyAdoRepositoryPage
{
    public class AddPolicyAdoRepositoryPageForm
    {
        [Required]
        public string Collection { get; set; } = string.Empty;
        [Required]
        public string Project { get; set; } = string.Empty;
        [Required]
        public string Repository { get; set; } = string.Empty;

        public string? SolutionPathMask { get; set; }
    }

    [Parameter] public Guid PolicyId { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IPolicyRepositoryService PolicyRepositoryService { get; set; } = null!;

    private readonly AddPolicyAdoRepositoryPageForm _formData = new AddPolicyAdoRepositoryPageForm();

    private async Task AddPolicyRepository()
    {
        await PolicyRepositoryService.AddAdoRepository(PolicyId, _formData.Collection, _formData.Project, _formData.Repository, _formData.SolutionPathMask);
        NavigationManager.NavigateTo($"/validation-policies/{PolicyId}");
    }
}