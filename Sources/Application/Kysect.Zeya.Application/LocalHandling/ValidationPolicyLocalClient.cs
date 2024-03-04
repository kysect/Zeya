using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.Dtos;

namespace Kysect.Zeya.Application.LocalHandling;

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

    public async Task<ValidationPolicyDto> GetPolicy(Guid id)
    {
        ValidationPolicyEntity policy = await _service.GetPolicy(id);
        return new ValidationPolicyDto(policy.Id, policy.Name, policy.Content);
    }

    public async Task<IReadOnlyCollection<ValidationPolicyDto>> GetPolicies()
    {
        IReadOnlyCollection<ValidationPolicyEntity> policyEntities = await _service.ReadPolicies();
        return policyEntities.Select(policyEntity => new ValidationPolicyDto(policyEntity.Id, policyEntity.Name, policyEntity.Content)).ToList();
    }

    public async Task<IReadOnlyCollection<RepositoryDiagnosticTableRow>> GetDiagnosticsTable(Guid policyId)
    {
        return await _service.GetDiagnosticsTable(policyId);
    }
}