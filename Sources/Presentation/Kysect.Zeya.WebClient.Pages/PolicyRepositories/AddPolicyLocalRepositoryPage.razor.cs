using Kysect.Zeya.Client.Abstractions;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Kysect.Zeya.WebClient.Pages.PolicyRepositories;

public partial class AddPolicyLocalRepositoryPage
{
    public class AddPolicyLocalRepositoryForm
    {
        [Required]
        public string Path { get; set; } = string.Empty;
    }

    [Parameter] public Guid PolicyId { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IPolicyRepositoryService PolicyRepositoryService { get; set; } = null!;

    private readonly AddPolicyLocalRepositoryForm _formData = new AddPolicyLocalRepositoryForm();

    private async Task AddPolicyRepository()
    {
        await PolicyRepositoryService.AddLocalRepository(PolicyId, _formData.Path);
        NavigationManager.NavigateTo($"/validation-policies/{PolicyId}");
    }
}