using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ValidationRules.Rules;

namespace Kysect.Zeya.RepositoryValidation;

public interface IValidationRuleFixerApplier
{
    bool IsFixerRegistered(IValidationRule rule);
    void Apply(IValidationRule rule, IClonedRepository clonedRepository);
}