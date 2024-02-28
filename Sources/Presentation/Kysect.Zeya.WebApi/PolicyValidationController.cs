using Kysect.Zeya.Client.Abstractions.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Kysect.Zeya.WebApi;

[ApiController]
[Route("Policy")]
public class PolicyValidationController : Controller
{
    private readonly IPolicyValidationApi _policyValidationApi;

    public PolicyValidationController(IPolicyValidationApi policyValidationApi)
    {
        _policyValidationApi = policyValidationApi;
    }


    [HttpPost("{PolicyId}/Validate")]
    public async Task<ActionResult> Validate(Guid policyId)
    {
        await _policyValidationApi.Validate(policyId);
        return Ok();
    }
}