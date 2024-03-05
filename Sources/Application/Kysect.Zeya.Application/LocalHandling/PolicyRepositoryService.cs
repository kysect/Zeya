using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.Dtos;

namespace Kysect.Zeya.Application.LocalHandling;

public class PolicyRepositoryService(
    ValidationPolicyService service,
    ValidationPolicyRepositoryFactory repositoryFactory)
    : IPolicyRepositoryService
{
    public async Task<ValidationPolicyRepositoryDto> AddRepository(Guid policyId, string githubOwner, string githubRepository)
    {
        ValidationPolicyRepository repository = await service.AddRepository(policyId, githubOwner, githubRepository);
        return repositoryFactory.Create(repository).ToDto();
    }

    public async Task<IReadOnlyCollection<ValidationPolicyRepositoryDto>> GetRepositories(Guid policyId)
    {
        IReadOnlyCollection<ValidationPolicyRepository> repositories = await service.GetRepositories(policyId);
        return repositories
            .Select(r => repositoryFactory.Create(r).ToDto())
            .ToList();
    }
}