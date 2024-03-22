using Kysect.CommonLib.BaseTypes.Extensions;
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

        var fixedRules = new List<IValidationRule>();
        Dictionary<string, IValidationRule> validationRules = rules.ToDictionary(r => r.DiagnosticCode, r => r);

        foreach (string code in validationRuleCodeForFix)
        {
            IValidationRule validationRule = validationRules[code];

            if (validationRuleFixerApplier.IsFixerRegistered(validationRule))
            {
                logger.LogInformation("Apply code fixer for {Code}", code);
                validationRuleFixerApplier.Apply(validationRule, localRepository);
                fixedRules.Add(validationRule);
            }
            else
            {
                logger.LogDebug("Fixer for {Code} is not available", code);
            }
        }

        return fixedRules;
    }
}