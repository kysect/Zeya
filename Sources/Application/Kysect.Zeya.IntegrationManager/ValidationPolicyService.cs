using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.IntegrationManager.Models;
using Kysect.Zeya.RepositoryValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kysect.Zeya.IntegrationManager;

public class ValidationPolicyService(IDbContextFactory<ZeyaDbContext> dbContextFactory)
{
    public ValidationPolicyEntity CreatePolicy(string name, string content)
    {
        using ZeyaDbContext context = dbContextFactory.CreateDbContext();

        EntityEntry<ValidationPolicyEntity> createdPolicy = context.ValidationPolicies.Add(new ValidationPolicyEntity(Guid.NewGuid(), name, content));

        context.SaveChanges();
        return createdPolicy.Entity;
    }

    public IReadOnlyCollection<ValidationPolicyEntity> ReadPolicies()
    {
        using ZeyaDbContext context = dbContextFactory.CreateDbContext();

        return context.ValidationPolicies.ToList();
    }

    public ValidationPolicyRepository AddRepository(Guid policyId, string githubOwner, string githubRepository)
    {
        using ZeyaDbContext context = dbContextFactory.CreateDbContext();

        ValidationPolicyEntity? policy = context.ValidationPolicies.Find(policyId);
        if (policy is null)
            throw new ArgumentException("Policy not found");

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
            .Select(d => new ValidationPolicyRepositoryDiagnostic(repository.Id, d.Code, d.Severity.ToString()))
            .ToList();

        context.ValidationPolicyRepositoryDiagnostics.AddRange(diagnostics);
        context.SaveChanges();
    }

    public IReadOnlyCollection<string> GetAllRulesForPolicy(Guid policyId)
    {
        using ZeyaDbContext context = dbContextFactory.CreateDbContext();

        return context
            .ValidationPolicyRepositories
            .Join(context.ValidationPolicyRepositoryDiagnostics,
                r => r.Id,
                d => d.ValidationPolicyRepositoryId,
                (r, d) => new { Repository = r, Diagnostic = d })
            .Where(t => t.Repository.ValidationPolicyId == policyId)
            .Select(t => t.Diagnostic.RuleId)
            .Distinct()
            .ToList();
    }

    public IReadOnlyCollection<ValidationPolicyRepositoryDiagnostic> GetDiagnostics(Guid repositoryId)
    {
        using ZeyaDbContext context = dbContextFactory.CreateDbContext();

        return context.ValidationPolicyRepositoryDiagnostics.Where(d => d.ValidationPolicyRepositoryId == repositoryId).ToList();
    }

    public IReadOnlyCollection<RepositoryDiagnosticTableRow> GetDiagnosticsTable(Guid policyId)
    {
        using ZeyaDbContext context = dbContextFactory.CreateDbContext();

        var diagnostics = context
            .ValidationPolicyRepositories
            .Join(context.ValidationPolicyRepositoryDiagnostics,
                r => r.Id,
                d => d.ValidationPolicyRepositoryId,
                (r, d) => new
                {
                    Repository = r,
                    Diagnostic = d
                })
            .Where(t => t.Repository.ValidationPolicyId == policyId)
            .Select(t => new
            {
                RepositoryId = t.Repository.Id,
                RepositoryName = $"{t.Repository.GithubOwner}/{t.Repository.GithubRepository}",
                t.Diagnostic.RuleId,
                t.Diagnostic.Severity
            })
            .ToList();

        var groupedDiagnostics = new List<RepositoryDiagnosticTableRow>();
        foreach (var group in diagnostics.GroupBy(d => d.RepositoryId))
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var item in group)
                dictionary[item.RuleId] = item.Severity;

            var row = new RepositoryDiagnosticTableRow(group.First().RepositoryName, dictionary);
            groupedDiagnostics.Add(row);
        }

        return groupedDiagnostics;
    }
}