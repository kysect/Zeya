using Kysect.Zeya.Client.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Kysect.Zeya.WebApi;

[ApiController]
[Route("Policy")]
public class PolicyValidationController : Controller
{
    private readonly IPolicyValidationService _policyValidationService;

    public PolicyValidationController(IPolicyValidationService policyValidationService)
    {
        _policyValidationService = policyValidationService;
    }


    [HttpPost("{PolicyId}/Validate")]
    public async Task<ActionResult> Validate(Guid policyId)
    {
        await _policyValidationService.Validate(policyId);
        return Ok();
    }

    [HttpPost("/Policy/{PolicyId}/Create-fix")]
    public async Task<ActionResult> CreateFix(Guid policyId, string repositoryOwner, string repositoryName)
    {
        await _policyValidationService.CreateFix(policyId, repositoryOwner, repositoryName);
        return Ok();
    }
}