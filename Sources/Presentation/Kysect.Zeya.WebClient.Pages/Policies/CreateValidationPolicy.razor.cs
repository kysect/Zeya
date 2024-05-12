using Kysect.Zeya.Client.Abstractions;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.WebClient.Pages.Policies;

public partial class CreateValidationPolicy
{
    public class CreateValidationPolicyForm
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
    }

    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private IPolicyService PolicyService { get; set; } = null!;

    private readonly CreateValidationPolicyForm _formData = new CreateValidationPolicyForm();

    private async Task CreatePolicy()
    {
        await PolicyService.CreatePolicy(_formData.Name, _formData.Content);
        _navigationManager.NavigateTo("validation-policies");
    }
}