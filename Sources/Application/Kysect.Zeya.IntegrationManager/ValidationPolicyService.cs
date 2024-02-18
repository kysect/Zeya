using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.RepositoryValidation;
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

    public ValidationPolicyRepository AddRepository(Guid policyId, string githubOwner, string githubRepository)
    {
        using ZeyaDbContext context = dbContextFactory.CreateDbContext();

        ValidationPolicyEntity? policy = context.ValidationPolicies.Find(policyId);
        if (policy is null)
        {
            throw new ArgumentException("Policy not found");
        }

        var repository = new ValidationPolicyRepository(Guid.NewGuid(), policyId, githubOwner, githubRepository);
        context.ValidationPolicyRepositories.Add(repository);
        context.SaveChanges();

        return repository;
    }

    public IReadOnlyCollection<ValidationPolicyRepository> GetRepositories(Guid policyId)
    {
        using ZeyaDbContext context = dbContextFactory.CreateDbContext();

        return context.ValidationPolicyRepositories.Where(r => r.ValidationPolicyId == policyId).ToList();
    }

    public void SaveReport(ValidationPolicyRepository repository, RepositoryValidationReport report)
    {
        report.ThrowIfNull();

        using ZeyaDbContext context = dbContextFactory.CreateDbContext();

        IQueryable<ValidationPolicyRepositoryDiagnostic> oldPolicyDiagnostics = context.ValidationPolicyRepositoryDiagnostics.Where(d => d.ValidationPolicyRepositoryId == repository.Id);
        context.ValidationPolicyRepositoryDiagnostics.RemoveRange(oldPolicyDiagnostics);

        var diagnostics = report
            .Diagnostics
            .Select(d => new ValidationPolicyRepositoryDiagnostic(repository.Id, d.Code, d.Message))
            .ToList();

        context.ValidationPolicyRepositoryDiagnostics.AddRange(diagnostics);
        context.SaveChanges();
    }
}