using Kysect.Zeya.Dtos;
using Refit;

namespace Kysect.Zeya.Client.Abstractions;

public interface IPolicyRepositoryService
{
    [Post("/Policies/{policyId}/Repositories")]
    Task<ValidationPolicyRepositoryDto> AddRepository(Guid policyId, string githubOwner, string githubRepository);

    [Get("/Policies/{policyId}/Repositories")]
    Task<IReadOnlyCollection<ValidationPolicyRepositoryDto>> GetRepositories(Guid policyId);
}