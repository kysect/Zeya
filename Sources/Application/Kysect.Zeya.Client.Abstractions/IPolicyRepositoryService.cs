using Kysect.Zeya.Dtos;
using Refit;

namespace Kysect.Zeya.Client.Abstractions;

public interface IPolicyRepositoryService
{
    [Post("/Policies/{policyId}/Repositories/Github")]
    Task<ValidationPolicyRepositoryDto> AddGithubRepository(Guid policyId, string githubOwner, string githubRepository, string? solutionPathMask);

    [Post("/Policies/{policyId}/Repositories/Local")]
    Task<ValidationPolicyRepositoryDto> AddLocalRepository(Guid policyId, string path, string? solutionPathMask);
    [Post("/Policies/{policyId}/Repositories/Ado")]
    Task<ValidationPolicyRepositoryDto> AddAdoRepository(Guid policyId, string collection, string project, string repository, string? solutionPathMask);

    [Post("/Policies/{policyId}/Repositories/Remote")]
    Task<ValidationPolicyRepositoryDto> AddRemoteRepository(Guid policyId, string remoteHttpUrl, string? solutionPathMask);


    [Get("/Policies/{policyId}/Repositories")]
    Task<IReadOnlyCollection<ValidationPolicyRepositoryDto>> GetRepositories(Guid policyId);

    [Get("/Policies/{policyId}/Repositories/{repositoryId}")]
    Task<ValidationPolicyRepositoryDto> GetRepository(Guid policyId, Guid repositoryId);

    [Get("/Policies/{policyId}/Repositories/{repositoryId}/GetActions")]
    Task<IReadOnlyCollection<ValidationPolicyRepositoryActionDto>> GetActions(Guid policyId, Guid repositoryId);
}