﻿using Kysect.Zeya.Client.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Kysect.Zeya.WebApi;

[ApiController]
[Route("Policies")]
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

    [HttpPost("{PolicyId}/{RepositoryId}/Fix")]
    public async Task<ActionResult> CreateFix(Guid policyId, Guid repositoryId)
    {
        await _policyValidationService.CreateFix(policyId, repositoryId);
        return Ok();
    }

    [HttpPost("{PolicyId}/{RepositoryId}/PreviewChanges")]
    public async Task<ActionResult<string>> PreviewChanges(Guid policyId, Guid repositoryId)
    {
        string diff = await _policyValidationService.PreviewChanges(policyId, repositoryId);
        return Ok(diff);
    }
}