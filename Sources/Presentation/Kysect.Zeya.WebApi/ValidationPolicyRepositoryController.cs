using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Kysect.Zeya.WebApi;

[ApiController]
[Route("Policies")]
public class ValidationPolicyRepositoryController : Controller
{
    private readonly IPolicyRepositoryService _policyService;

    public ValidationPolicyRepositoryController(IPolicyRepositoryService policyService)
    {
        _policyService = policyService;
    }

    [HttpPost("{policyId}/Repositories")]
    public async Task<ActionResult<ValidationPolicyRepositoryDto>> AddRepository(Guid policyId, string githubOwner, string githubRepository)
    {
        ValidationPolicyRepositoryDto result = await _policyService.AddRepository(policyId, githubOwner, githubRepository);
        return Ok(result);
    }

    [HttpGet("{policyId}/Repositories")]
    public async Task<ActionResult<IReadOnlyCollection<ValidationPolicyRepositoryDto>>> GetRepositories(Guid policyId)
    {
        IReadOnlyCollection<ValidationPolicyRepositoryDto> result = await _policyService.GetRepositories(policyId);
        return Ok(result);
    }
}