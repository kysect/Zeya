using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Client.Abstractions.Models;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.IntegrationManager;

namespace Kysect.Zeya.Client.Local;

public class ValidationPolicyLocalClient : IValidationPolicyApi
{
    private readonly ValidationPolicyService _service;

    public ValidationPolicyLocalClient(ValidationPolicyService service)
    {
        _service = service;
    }

    public async Task<ValidationPolicyDto> CreatePolicy(string name, string content)
    {
        ValidationPolicyEntity policyEntity = await _service.CreatePolicy(name, content);
        return new ValidationPolicyDto(policyEntity.Id, policyEntity.Name, policyEntity.Content);
    }

    public async Task<IReadOnlyCollection<ValidationPolicyDto>> ReadPolicies()
    {
        IReadOnlyCollection<ValidationPolicyEntity> policyEntities = await _service.ReadPolicies();
        return policyEntities.Select(policyEntity => new ValidationPolicyDto(policyEntity.Id, policyEntity.Name, policyEntity.Content)).ToList();
    }

    public async Task<ValidationPolicyRepositoryDto> AddRepository(Guid policyId, string githubOwner, string githubRepository)
    {
        ValidationPolicyRepository repository = await _service.AddRepository(policyId, githubOwner, githubRepository);
        return new ValidationPolicyRepositoryDto(repository.Id, repository.ValidationPolicyId, repository.GithubOwner, repository.GithubRepository);
    }

    public async Task<IReadOnlyCollection<RepositoryDiagnosticTableRow>> GetDiagnosticsTable(Guid policyId)
    {
        return await _service.GetDiagnosticsTable(policyId);
    }

    public async Task<IReadOnlyCollection<ValidationPolicyRepositoryDto>> GetRepositories(Guid policyId)
    {
        IReadOnlyCollection<ValidationPolicyRepository> repositories = await _service.GetRepositories(policyId);
        return repositories
            .Select(r => new ValidationPolicyRepositoryDto(r.Id, r.ValidationPolicyId, r.GithubOwner, r.GithubRepository))
            .ToList();
    }
}
