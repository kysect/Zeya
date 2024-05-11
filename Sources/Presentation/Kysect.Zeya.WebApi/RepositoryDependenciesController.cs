using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Kysect.Zeya.WebApi;

[ApiController]
[Route("/RepositoryDependencies")]
public class RepositoryDependenciesController(IRepositoryDependenciesService service) : Controller
{
    [HttpGet("Tree")]
    public async Task<ActionResult<string>> GetRepositoryDependenciesTree(Guid policyId)
    {
        string result = await service.GetRepositoryDependenciesTree(policyId);
        return Ok(result);
    }

    [HttpGet("ActionPlan")]
    public async Task<ActionResult<IReadOnlyCollection<FixingActionPlanRow>>> GetFixingActionPlan(Guid policyId)
    {
        var result = await service.GetFixingActionPlan(policyId);
        return Ok(result);
    }
}