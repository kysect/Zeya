using Kysect.Zeya.Client.Abstractions;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.WebClient.Pages.PolicyRepositories;

public partial class AddPolicyGithubRepositoryPage
{
    public class AddPolicyGithubRepositoryForm
    {
        [Required]
        public string GithubOwner { get; set; } = string.Empty;
        [Required]
        public string GithubRepository { get; set; } = string.Empty;
        public string? SolutionPathMask { get; set; }
    }

    [Parameter] public Guid PolicyId { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IPolicyRepositoryService PolicyRepositoryService { get; set; } = null!;

    private readonly AddPolicyGithubRepositoryForm _formData = new AddPolicyGithubRepositoryForm();

    private async Task AddPolicyRepository()
    {
        await PolicyRepositoryService.AddGithubRepository(PolicyId, _formData.GithubOwner, _formData.GithubRepository, _formData.SolutionPathMask);
        NavigationManager.NavigateTo($"/validation-policies/{PolicyId}");
    }
}