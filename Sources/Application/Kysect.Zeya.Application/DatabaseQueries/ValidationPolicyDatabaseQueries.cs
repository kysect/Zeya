﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.DataAccess.EntityFramework.Tools;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.ModelMapping;
using Kysect.Zeya.RepositoryValidation.ProcessingActions;
using Microsoft.EntityFrameworkCore;

namespace Kysect.Zeya.Application.DatabaseQueries;

public class ValidationPolicyDatabaseQueries
{
    private readonly ZeyaDbContext _context;

    public ValidationPolicyDatabaseQueries(ZeyaDbContext context)
    {
        _context = context;
    }

    public async Task<ValidationPolicyEntity> GetPolicy(Guid id)
    {
        return await _context
            .ValidationPolicies
            .GetAsync(id);
    }

    public async Task SaveValidationResults(Guid repositoryId, IReadOnlyCollection<RepositoryProcessingMessage> repositoryProcessingMessages)
    {
        await SaveReport(repositoryId, repositoryProcessingMessages);
    }

    public async Task SaveReport(Guid repositoryId, IReadOnlyCollection<RepositoryProcessingMessage> repositoryProcessingMessages)
    {
        IQueryable<ValidationPolicyRepositoryDiagnostic> oldPolicyDiagnostics = _context
            .ValidationPolicyRepositoryDiagnostics
            .Where(d => d.ValidationPolicyRepositoryId == repositoryId);
        _context.ValidationPolicyRepositoryDiagnostics.RemoveRange(oldPolicyDiagnostics);

        List<ValidationPolicyRepositoryDiagnostic> diagnostics = repositoryProcessingMessages
            .Select(d => new ValidationPolicyRepositoryDiagnostic(repositoryId, d.Code, RepositoryValidationSeverityMapping.ToApplicationModel(d.Severity)))
            .DistinctBy(d => d.RuleId)
            .ToList();

        await _context.ValidationPolicyRepositoryDiagnostics.AddRangeAsync(diagnostics);
        await _context.SaveChangesAsync();
    }

    public async Task SaveProcessingActionResult<T>(Guid repositoryId, RepositoryProcessingResponse<T> response)
    {
        response.ThrowIfNull();
        if (response.Messages.IsEmpty())
            return;

        var actionId = Guid.NewGuid();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var validationPolicyRepositoryAction = new ValidationPolicyRepositoryAction(actionId, repositoryId, response.ActionName, now);
        List<ValidationPolicyRepositoryActionMessage> messages = response.Messages
            .Select(e => new ValidationPolicyRepositoryActionMessage(Guid.NewGuid(), actionId, $"{e.Code}: {e.Message}"))
            .ToList();

        _context.ValidationPolicyRepositoryActions.Add(validationPolicyRepositoryAction);
        _context.ValidationPolicyRepositoryActionMessages.AddRange(messages);
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
                RepositoryName = t.Repository.Metadata,
                t.Diagnostic.RuleId,
                t.Diagnostic.Severity
            })
            .ToListAsync();

        var groupedDiagnostics = new List<RepositoryDiagnosticTableRow>();
        foreach (var group in diagnostics.GroupBy(d => d.RepositoryId))
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var item in group)
                dictionary[item.RuleId] = item.Severity.ToString();

            var row = new RepositoryDiagnosticTableRow(group.First().RepositoryId, group.First().RepositoryName, dictionary);
            groupedDiagnostics.Add(row);
        }

        return groupedDiagnostics;
    }
}
