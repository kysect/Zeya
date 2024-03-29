﻿using Kysect.Zeya.Dtos;
using Refit;

namespace Kysect.Zeya.Client.Abstractions;

public interface IPolicyRepositoryService
{
    [Post("/Policies/{policyId}/Repositories/Github")]
    Task<ValidationPolicyRepositoryDto> AddGithubRepository(Guid policyId, string githubOwner, string githubRepository, string? solutionPathMask);

    [Post("/Policies/{policyId}/Repositories/Local")]
    Task<ValidationPolicyRepositoryDto> AddLocalRepository(Guid policyId, string path, string? solutionPathMask);

    [Get("/Policies/{policyId}/Repositories")]
    Task<IReadOnlyCollection<ValidationPolicyRepositoryDto>> GetRepositories(Guid policyId);
}