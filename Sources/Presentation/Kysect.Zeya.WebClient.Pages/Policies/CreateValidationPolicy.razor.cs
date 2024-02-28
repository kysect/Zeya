using Kysect.Zeya.Client.Abstractions.Contracts;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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
    [Inject] private IValidationPolicyApi _validationPolicyApi { get; set; } = null!;

    private readonly CreateValidationPolicyForm _formData = new CreateValidationPolicyForm();

    private async Task CreatePolicy()
    {
        await _validationPolicyApi.CreatePolicy(_formData.Name, _formData.Content);
        _navigationManager.NavigateTo("validation-policies");
    }
}