using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.Dtos;

namespace Kysect.Zeya.Application.LocalHandling;

public class ValidationPolicyRepositoryApiLocalClient : IValidationPolicyRepositoryApi
{
    private readonly ValidationPolicyService _service;

    public ValidationPolicyRepositoryApiLocalClient(ValidationPolicyService service)
    {
        _service = service;
    }

    public async Task<ValidationPolicyRepositoryDto> AddRepository(Guid policyId, string githubOwner, string githubRepository)
    {
        ValidationPolicyRepository repository = await _service.AddRepository(policyId, githubOwner, githubRepository);
        return new ValidationPolicyRepositoryDto(repository.Id, repository.ValidationPolicyId, repository.GithubOwner, repository.GithubRepository);
    }

    public async Task<IReadOnlyCollection<ValidationPolicyRepositoryDto>> GetRepositories(Guid policyId)
    {
        IReadOnlyCollection<ValidationPolicyRepository> repositories = await _service.GetRepositories(policyId);
        return repositories
            .Select(r => new ValidationPolicyRepositoryDto(r.Id, r.ValidationPolicyId, r.GithubOwner, r.GithubRepository))
            .ToList();
    }
}