using Kysect.Zeya.Application.DatabaseQueries;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kysect.Zeya.Application.LocalHandling;

public class PolicyService(ValidationPolicyDatabaseQueries databaseQueries, ZeyaDbContext context)
    : IPolicyService
{
    public async Task<ValidationPolicyDto> CreatePolicy(string name, string content)
    {
        EntityEntry<ValidationPolicyEntity> createdPolicy = context.ValidationPolicies.Add(new ValidationPolicyEntity(Guid.NewGuid(), name, content));
        await context.SaveChangesAsync();

        ValidationPolicyEntity policyEntity = createdPolicy.Entity;
        return new ValidationPolicyDto(policyEntity.Id, policyEntity.Name, policyEntity.Content);
    }

    public async Task<ValidationPolicyDto> GetPolicy(Guid id)
    {
        ValidationPolicyEntity policy = await databaseQueries.GetPolicy(id);
        return new ValidationPolicyDto(policy.Id, policy.Name, policy.Content);
    }

    public async Task<IReadOnlyCollection<ValidationPolicyDto>> GetPolicies()
    {
        IReadOnlyCollection<ValidationPolicyEntity> policyEntities = await context.ValidationPolicies.ToListAsync();
        return policyEntities.Select(policyEntity => new ValidationPolicyDto(policyEntity.Id, policyEntity.Name, policyEntity.Content)).ToList();
    }

    public async Task<IReadOnlyCollection<RepositoryDiagnosticTableRow>> GetDiagnosticsTable(Guid policyId)
    {
        return await databaseQueries.GetDiagnosticsTable(policyId);
    }
}