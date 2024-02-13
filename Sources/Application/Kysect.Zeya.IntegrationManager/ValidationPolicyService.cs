using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kysect.Zeya.IntegrationManager;

public class ValidationPolicyService(IDbContextFactory<ZeyaDbContext> dbContextFactory)
{
    public ValidationPolicyEntity Create(string name, string content)
    {
        using ZeyaDbContext context = dbContextFactory.CreateDbContext();

        EntityEntry<ValidationPolicyEntity> createdPolicy = context.ValidationPolicies.Add(new ValidationPolicyEntity(Guid.NewGuid(), name, content));

        context.SaveChanges();
        return createdPolicy.Entity;
    }

    public IReadOnlyCollection<ValidationPolicyEntity> Read()
    {
        using ZeyaDbContext context = dbContextFactory.CreateDbContext();

        return context.ValidationPolicies.ToList();
    }
}