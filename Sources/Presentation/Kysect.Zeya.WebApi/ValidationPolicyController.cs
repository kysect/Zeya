using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Client.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kysect.Zeya.WebApi;

[ApiController]
[Route("[controller]")]
public class ValidationPolicyController : Controller
{
    private readonly IValidationPolicyApi _policyApi;

    public ValidationPolicyController(IValidationPolicyApi policyApi)
    {
        _policyApi = policyApi;
    }

    [HttpPost("Policies")]
    public async Task<ActionResult<ValidationPolicyDto>> CreatePolicy(string name, string content)
    {
        ValidationPolicyDto result = await _policyApi.CreatePolicy(name, content);
        return Ok(result);
    }

    [HttpGet("Policies")]
    public async Task<ActionResult<IReadOnlyCollection<ValidationPolicyDto>>> ReadPolicies()
    {
        IReadOnlyCollection<ValidationPolicyDto> result = await _policyApi.ReadPolicies();
        return Ok(result);
    }

    [HttpPost("Repository")]
    public async Task<ActionResult<ValidationPolicyRepositoryDto>> AddRepository(Guid policyId, string githubOwner, string githubRepository)
    {
        ValidationPolicyRepositoryDto result = await _policyApi.AddRepository(policyId, githubOwner, githubRepository);
        return Ok(result);
    }

    [HttpGet("DiagnosticsTables")]
    public async Task<ActionResult<IReadOnlyCollection<RepositoryDiagnosticTableRow>>> GetDiagnosticsTable(Guid policyId)
    {
        IReadOnlyCollection<RepositoryDiagnosticTableRow> result = await _policyApi.GetDiagnosticsTable(policyId);
        return Ok(result);
    }

    [HttpGet("Repositories")]
    public async Task<ActionResult<IReadOnlyCollection<ValidationPolicyRepositoryDto>>> GetRepositories(Guid policyId)
    {
        IReadOnlyCollection<ValidationPolicyRepositoryDto> result = await _policyApi.GetRepositories(policyId);
        return Ok(result);
    }
}
