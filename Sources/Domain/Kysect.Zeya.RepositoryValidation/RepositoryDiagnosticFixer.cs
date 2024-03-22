using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidation;

public class RepositoryDiagnosticFixer(IValidationRuleFixerApplier validationRuleFixerApplier, ILogger<RepositoryDiagnosticFixer> logger)
{
    public IReadOnlyCollection<IValidationRule> Fix(
        IReadOnlyCollection<IValidationRule> rules,
        ILocalRepository localRepository,
        IReadOnlyCollection<string> validationRuleCodeForFix)
    {
        rules.ThrowIfNull();
        localRepository.ThrowIfNull();
        validationRuleCodeForFix.ThrowIfNull();

        var fixedDiagnostics = new List<IValidationRule>();

        foreach (string code in validationRuleCodeForFix)
        {
            // TODO: rework this hack
            IValidationRule? validationRule = rules.FirstOrDefault(r => r.DiagnosticCode == code);
            if (validationRule is null)
                throw new DotnetProjectSystemException($"Rule {code} was not found");

            if (validationRuleFixerApplier.IsFixerRegistered(validationRule))
            {
                logger.LogInformation("Apply code fixer for {Code}", code);
                validationRuleFixerApplier.Apply(validationRule, localRepository);
                fixedDiagnostics.Add(validationRule);
            }
            else
            {
                logger.LogDebug("Fixer for {Code} is not available", code);
            }
        }

        return fixedDiagnostics;
    }
}