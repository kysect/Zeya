using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Client.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kysect.Zeya.WebApi;

[ApiController]
[Route("ValidationPolicy")]
public class ValidationPolicyRepositoryController : Controller
{
    private readonly IValidationPolicyRepositoryApi _policyApi;

    public ValidationPolicyRepositoryController(IValidationPolicyRepositoryApi policyApi)
    {
        _policyApi = policyApi;
    }

    [HttpPost("{policyId}/Repositories")]
    public async Task<ActionResult<ValidationPolicyRepositoryDto>> AddRepository(Guid policyId, string githubOwner, string githubRepository)
    {
        ValidationPolicyRepositoryDto result = await _policyApi.AddRepository(policyId, githubOwner, githubRepository);
        return Ok(result);
    }

    [HttpGet("{policyId}/Repositories")]
    public async Task<ActionResult<IReadOnlyCollection<ValidationPolicyRepositoryDto>>> GetRepositories(Guid policyId)
    {
        IReadOnlyCollection<ValidationPolicyRepositoryDto> result = await _policyApi.GetRepositories(policyId);
        return Ok(result);
    }
}