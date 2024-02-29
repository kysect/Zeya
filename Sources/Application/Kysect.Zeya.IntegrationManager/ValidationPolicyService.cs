using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.Client.Abstractions.Models;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.RepositoryValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kysect.Zeya.IntegrationManager;

public class ValidationPolicyService
{
    private readonly ZeyaDbContext _context;

    public ValidationPolicyService(ZeyaDbContext context)
    {
        _context = context;
    }

    public async Task<ValidationPolicyEntity> CreatePolicy(string name, string content)
    {
        EntityEntry<ValidationPolicyEntity> createdPolicy = _context.ValidationPolicies.Add(new ValidationPolicyEntity(Guid.NewGuid(), name, content));

        await _context.SaveChangesAsync();
        return createdPolicy.Entity;
    }

    public async Task<ValidationPolicyEntity> GetPolicy(Guid id)
    {
        // TODO: Handle null result
        ValidationPolicyEntity? result = await _context
            .ValidationPolicies
            .FindAsync(id);

        result.ThrowIfNull();

        return result;
    }

    public async Task<IReadOnlyCollection<ValidationPolicyEntity>> ReadPolicies()
    {
        return await _context.ValidationPolicies.ToListAsync();
    }

    public async Task<ValidationPolicyRepository> AddRepository(Guid policyId, string githubOwner, string githubRepository)
    {
        ValidationPolicyEntity? policy = await _context.ValidationPolicies.FindAsync(policyId);
        if (policy is null)
            throw new ArgumentException("Policy not found");

        var repository = new ValidationPolicyRepository(Guid.NewGuid(), policyId, githubOwner, githubRepository);
        _context.ValidationPolicyRepositories.Add(repository);
        await _context.SaveChangesAsync();

        return repository;
    }

    public async Task<IReadOnlyCollection<ValidationPolicyRepository>> GetRepositories(Guid policyId)
    {
        return await _context.ValidationPolicyRepositories.Where(r => r.ValidationPolicyId == policyId).ToListAsync();
    }

    public async Task<ValidationPolicyRepository> GetRepository(Guid policyId, string repositoryOwner, string repositoryName)
    {
        return await _context
            .ValidationPolicyRepositories
            .Where(r => r.ValidationPolicyId == policyId)
            .Where(r => r.GithubOwner == repositoryOwner)
            .Where(r => r.GithubRepository == repositoryName)
            .SingleAsync();
    }

    public async Task SaveReport(ValidationPolicyRepository repository, RepositoryValidationReport report)
    {
        report.ThrowIfNull();

        IQueryable<ValidationPolicyRepositoryDiagnostic> oldPolicyDiagnostics = _context.ValidationPolicyRepositoryDiagnostics.Where(d => d.ValidationPolicyRepositoryId == repository.Id);
        _context.ValidationPolicyRepositoryDiagnostics.RemoveRange(oldPolicyDiagnostics);

        var diagnostics = report
            .Diagnostics
            .Select(d => new ValidationPolicyRepositoryDiagnostic(repository.Id, d.Code, d.Severity.ToString()))
            .DistinctBy(d => d.RuleId)
            .ToList();

        await _context.ValidationPolicyRepositoryDiagnostics.AddRangeAsync(diagnostics);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<string>> GetAllRulesForPolicy(Guid policyId)
    {
        return await _context
            .ValidationPolicyRepositories
            .Join(_context.ValidationPolicyRepositoryDiagnostics,
                r => r.Id,
                d => d.ValidationPolicyRepositoryId,
                (r, d) => new { Repository = r, Diagnostic = d })
            .Where(t => t.Repository.ValidationPolicyId == policyId)
            .Select(t => t.Diagnostic.RuleId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<ValidationPolicyRepositoryDiagnostic>> GetDiagnostics(Guid repositoryId)
    {
        return await _context.ValidationPolicyRepositoryDiagnostics.Where(d => d.ValidationPolicyRepositoryId == repositoryId).ToListAsync();
    }

    public async Task<IReadOnlyCollection<RepositoryDiagnosticTableRow>> GetDiagnosticsTable(Guid policyId)
    {
        var diagnostics = await _context
            .ValidationPolicyRepositories
            .Join(_context.ValidationPolicyRepositoryDiagnostics,
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
                RepositoryOwner = t.Repository.GithubOwner,
                RepositoryName = t.Repository.GithubRepository,
                t.Diagnostic.RuleId,
                t.Diagnostic.Severity
            })
            .ToListAsync();

        var groupedDiagnostics = new List<RepositoryDiagnosticTableRow>();
        foreach (var group in diagnostics.GroupBy(d => d.RepositoryId))
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var item in group)
                dictionary[item.RuleId] = item.Severity;

            var row = new RepositoryDiagnosticTableRow(group.First().RepositoryOwner, group.First().RepositoryName, dictionary);
            groupedDiagnostics.Add(row);
        }

        return groupedDiagnostics;
    }
}