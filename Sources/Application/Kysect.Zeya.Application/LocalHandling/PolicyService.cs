using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kysect.Zeya.Application.LocalHandling;

public class PolicyService : IPolicyService
{
    private readonly ValidationPolicyService _service;
    private readonly ZeyaDbContext _context;

    public PolicyService(ValidationPolicyService service, ZeyaDbContext context)
    {
        _service = service;
        _context = context;
    }

    public async Task<ValidationPolicyDto> CreatePolicy(string name, string content)
    {
        EntityEntry<ValidationPolicyEntity> createdPolicy = _context.ValidationPolicies.Add(new ValidationPolicyEntity(Guid.NewGuid(), name, content));
        await _context.SaveChangesAsync();

        ValidationPolicyEntity policyEntity = createdPolicy.Entity;
        return new ValidationPolicyDto(policyEntity.Id, policyEntity.Name, policyEntity.Content);
    }

    public async Task<ValidationPolicyDto> GetPolicy(Guid id)
    {

        ValidationPolicyEntity policy = await _service.GetPolicy(id);
        return new ValidationPolicyDto(policy.Id, policy.Name, policy.Content);
    }

    public async Task<IReadOnlyCollection<ValidationPolicyDto>> GetPolicies()
    {
        IReadOnlyCollection<ValidationPolicyEntity> policyEntities = await _context.ValidationPolicies.ToListAsync();
        return policyEntities.Select(policyEntity => new ValidationPolicyDto(policyEntity.Id, policyEntity.Name, policyEntity.Content)).ToList();
    }

    public async Task<IReadOnlyCollection<RepositoryDiagnosticTableRow>> GetDiagnosticsTable(Guid policyId)
    {
        return await _service.GetDiagnosticsTable(policyId);
    }
}