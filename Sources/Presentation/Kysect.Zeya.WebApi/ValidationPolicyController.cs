using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Kysect.Zeya.WebApi;

[ApiController]
[Route("[controller]")]
public class ValidationPolicyController : Controller
{
    private readonly IPolicyService _policyService;

    public ValidationPolicyController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpPost("")]
    public async Task<ActionResult<ValidationPolicyDto>> CreatePolicy(string name, string content)
    {
        ValidationPolicyDto result = await _policyService.CreatePolicy(name, content);
        return Ok(result);
    }

    [HttpGet("")]
    public async Task<ActionResult<IReadOnlyCollection<ValidationPolicyDto>>> ReadPolicies()
    {
        IReadOnlyCollection<ValidationPolicyDto> result = await _policyService.GetPolicies();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IReadOnlyCollection<ValidationPolicyDto>>> GetPolicy(Guid id)
    {
        ValidationPolicyDto result = await _policyService.GetPolicy(id);
        return Ok(result);
    }

    [HttpGet("{policyId}/DiagnosticsTables")]
    public async Task<ActionResult<IReadOnlyCollection<RepositoryDiagnosticTableRow>>> GetDiagnosticsTable(Guid policyId)
    {
        IReadOnlyCollection<RepositoryDiagnosticTableRow> result = await _policyService.GetDiagnosticsTable(policyId);
        return Ok(result);
    }
}
