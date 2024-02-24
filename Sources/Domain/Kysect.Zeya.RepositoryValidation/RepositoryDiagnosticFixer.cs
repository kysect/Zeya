﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidation;

public class RepositoryDiagnosticFixer(IValidationRuleFixerApplier validationRuleFixerApplier, ILogger<RepositoryDiagnosticFixer> logger)
{
    public IReadOnlyCollection<IValidationRule> Fix(RepositoryValidationReport report, IReadOnlyCollection<IValidationRule> rules, ILocalRepository localRepository)
    {
        report.ThrowIfNull();
        rules.ThrowIfNull();
        localRepository.ThrowIfNull();

        var fixedDiagnostics = new List<IValidationRule>();

        foreach (IGrouping<string, RepositoryValidationDiagnostic> grouping in report.Diagnostics.GroupBy(d => d.Code))
        {
            RepositoryValidationDiagnostic diagnostic = grouping.First();
            // TODO: rework this hack
            IValidationRule? validationRule = rules.FirstOrDefault(r => r.DiagnosticCode == diagnostic.Code);
            if (validationRule is null)
                throw new DotnetProjectSystemException($"Rule {diagnostic.Code} was not found");

            if (validationRuleFixerApplier.IsFixerRegistered(validationRule))
            {
                logger.LogInformation("Apply code fixer for {Code}", diagnostic.Code);
                validationRuleFixerApplier.Apply(validationRule, localRepository);
                fixedDiagnostics.Add(validationRule);
            }
            else
            {
                logger.LogDebug("Fixer for {Code} is not available", diagnostic.Code);
            }
        }

        return fixedDiagnostics;
    }
}