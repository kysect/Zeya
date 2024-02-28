using Kysect.Zeya.Client.Abstractions.Models;
using Refit;

namespace Kysect.Zeya.Client.Abstractions.Contracts;

public interface IValidationPolicyRepositoryApi
{
    [Post("/ValidationPolicy/{policyId}/Repositories")]
    Task<ValidationPolicyRepositoryDto> AddRepository(Guid policyId, string githubOwner, string githubRepository);

    [Get("/ValidationPolicy/{policyId}/Repositories")]
    Task<IReadOnlyCollection<ValidationPolicyRepositoryDto>> GetRepositories(Guid policyId);
}