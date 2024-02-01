using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.RepositoryValidation;

public class RepositoryDiagnosticFixer(IValidationRuleFixerApplier validationRuleFixerApplier, ILogger logger)
{
    public IReadOnlyCollection<IValidationRule> Fix(RepositoryValidationReport report, IReadOnlyCollection<IValidationRule> rules, IClonedRepository clonedRepository)
    {
        report.ThrowIfNull();
        rules.ThrowIfNull();
        clonedRepository.ThrowIfNull();

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
                validationRuleFixerApplier.Apply(validationRule, clonedRepository);
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