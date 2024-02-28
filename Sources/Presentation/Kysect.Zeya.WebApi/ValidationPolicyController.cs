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

    [HttpPost("")]
    public async Task<ActionResult<ValidationPolicyDto>> CreatePolicy(string name, string content)
    {
        ValidationPolicyDto result = await _policyApi.CreatePolicy(name, content);
        return Ok(result);
    }

    [HttpGet("")]
    public async Task<ActionResult<IReadOnlyCollection<ValidationPolicyDto>>> ReadPolicies()
    {
        IReadOnlyCollection<ValidationPolicyDto> result = await _policyApi.GetPolicies();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IReadOnlyCollection<ValidationPolicyDto>>> GetPolicy(Guid id)
    {
        ValidationPolicyDto result = await _policyApi.GetPolicy(id);
        return Ok(result);
    }

    [HttpGet("{policyId}/DiagnosticsTables")]
    public async Task<ActionResult<IReadOnlyCollection<RepositoryDiagnosticTableRow>>> GetDiagnosticsTable(Guid policyId)
    {
        IReadOnlyCollection<RepositoryDiagnosticTableRow> result = await _policyApi.GetDiagnosticsTable(policyId);
        return Ok(result);
    }
}
