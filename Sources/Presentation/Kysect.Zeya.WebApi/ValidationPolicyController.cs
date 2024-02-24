using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Client.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kysect.Zeya.WebApi;

[ApiController]
[Route("[controller]")]
public class ValidationPolicyController(IValidationPolicyApi policyApi) : Controller
{
    [HttpPost("Policies")]
    public ActionResult<ValidationPolicyDto> CreatePolicy(string name, string content)
    {
        ValidationPolicyDto result = policyApi.CreatePolicy(name, content);
        return Ok(result);
    }

    [HttpGet("Policies")]
    public ActionResult<IReadOnlyCollection<ValidationPolicyDto>> ReadPolicies()
    {
        IReadOnlyCollection<ValidationPolicyDto> result = policyApi.ReadPolicies();
        return Ok(result);
    }

    [HttpPost("Repository")]
    public ActionResult<ValidationPolicyRepositoryDto> AddRepository(Guid policyId, string githubOwner, string githubRepository)
    {
        ValidationPolicyRepositoryDto result = policyApi.AddRepository(policyId, githubOwner, githubRepository);
        return Ok(result);
    }

    [HttpGet("DiagnosticsTables")]
    public ActionResult<IReadOnlyCollection<RepositoryDiagnosticTableRow>> GetDiagnosticsTable(Guid policyId)
    {
        IReadOnlyCollection<RepositoryDiagnosticTableRow> result = policyApi.GetDiagnosticsTable(policyId);
        return Ok(result);
    }

    [HttpGet("Repositories")]
    public ActionResult<IReadOnlyCollection<ValidationPolicyRepositoryDto>> GetRepositories(Guid policyId)
    {
        IReadOnlyCollection<ValidationPolicyRepositoryDto> result = policyApi.GetRepositories(policyId);
        return Ok(result);
    }
}