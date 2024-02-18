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

    public ValidationPolicyDto CreatePolicy(string name, string content)
    {
        ValidationPolicyEntity policyEntity = _service.CreatePolicy(name, content);
        return new ValidationPolicyDto(policyEntity.Id, policyEntity.Name, policyEntity.Content);
    }

    public IReadOnlyCollection<ValidationPolicyDto> ReadPolicies()
    {
        IReadOnlyCollection<ValidationPolicyEntity> policyEntities = _service.ReadPolicies();
        return policyEntities.Select(policyEntity => new ValidationPolicyDto(policyEntity.Id, policyEntity.Name, policyEntity.Content)).ToList();
    }

    public ValidationPolicyRepositoryDto AddRepository(Guid policyId, string githubOwner, string githubRepository)
    {
        ValidationPolicyRepository repository = _service.AddRepository(policyId, githubOwner, githubRepository);
        return new ValidationPolicyRepositoryDto(repository.Id, repository.ValidationPolicyId, repository.GithubOwner, repository.GithubRepository);
    }

    public IReadOnlyCollection<RepositoryDiagnosticTableRow> GetDiagnosticsTable(Guid policyId)
    {
        return _service.GetDiagnosticsTable(policyId);
    }
}
