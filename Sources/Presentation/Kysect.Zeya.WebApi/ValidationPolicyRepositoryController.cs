﻿using Kysect.Zeya.Client.Abstractions;
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

    [HttpPost("{policyId}/Repositories/Github")]
    public async Task<ActionResult<ValidationPolicyRepositoryDto>> AddGithubRepository(Guid policyId, string githubOwner, string githubRepository, string? solutionPathMask)
    {
        ValidationPolicyRepositoryDto result = await _policyService.AddGithubRepository(policyId, githubOwner, githubRepository, solutionPathMask);
        return Ok(result);
    }

    [HttpPost("{policyId}/Repositories/Local")]
    public async Task<ActionResult<ValidationPolicyRepositoryDto>> AddLocalRepository(Guid policyId, string path, string? solutionPathMask)
    {
        ValidationPolicyRepositoryDto result = await _policyService.AddLocalRepository(policyId, path, solutionPathMask);
        return Ok(result);
    }

    [HttpPost("{policyId}/Repositories/Ado")]
    public async Task<ActionResult<ValidationPolicyRepositoryDto>> AddAdoRepository(Guid policyId, string collection, string project, string repository, string? solutionPathMask)
    {
        ValidationPolicyRepositoryDto result = await _policyService.AddAdoRepository(policyId, collection, project, repository, solutionPathMask);
        return Ok(result);
    }

    [HttpPost("{policyId}/Repositories/Remote")]
    public async Task<ActionResult<ValidationPolicyRepositoryDto>> AddRemoteRepository(Guid policyId, string remoteHttpUrl, string? solutionPathMask)
    {
        ValidationPolicyRepositoryDto result = await _policyService.AddRemoteRepository(policyId, remoteHttpUrl, solutionPathMask);
        return Ok(result);
    }

    [HttpGet("{policyId}/Repositories")]
    public async Task<ActionResult<IReadOnlyCollection<ValidationPolicyRepositoryDto>>> GetRepositories(Guid policyId)
    {
        IReadOnlyCollection<ValidationPolicyRepositoryDto> result = await _policyService.GetRepositories(policyId);
        return Ok(result);
    }

    [HttpGet("/Policies/{policyId}/Repositories/{repositoryId}")]
    public async Task<ActionResult<ValidationPolicyRepositoryDto>> GetRepository(Guid policyId, Guid repositoryId)
    {
        ValidationPolicyRepositoryDto result = await _policyService.GetRepository(policyId, repositoryId);
        return Ok(result);
    }

    [HttpGet("/Policies/{policyId}/Repositories/{repositoryId}/GetActions")]
    public async Task<ActionResult<IReadOnlyCollection<ValidationPolicyRepositoryActionDto>>> GetActions(Guid policyId, Guid repositoryId)
    {
        IReadOnlyCollection<ValidationPolicyRepositoryActionDto> result = await _policyService.GetActions(policyId, repositoryId);
        return Ok(result);
    }
}