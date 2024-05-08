using Kysect.Zeya.Client.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Kysect.Zeya.WebApi;

[ApiController]
[Route("/RepositoryDependencies")]
public class RepositoryDependenciesController(IRepositoryDependenciesService service) : Controller
{
    [HttpGet("")]
    public async Task<ActionResult<string>> GetRepositoryDependenciesTree(Guid policyId)
    {
        string result = await service.GetRepositoryDependenciesTree(policyId);
        return Ok(result);
    }
}