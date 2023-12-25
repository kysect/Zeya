using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ValidationRules.Rules;

namespace Kysect.Zeya.ValidationRules.Fixers;

public interface IValidationRuleFixerApplier
{
    bool IsFixerRegistered(IValidationRule rule);
    void Apply(IValidationRule rule, IClonedRepository clonedRepository);
}