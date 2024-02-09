using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.ValidationRules.Abstractions;

namespace Kysect.Zeya.RepositoryValidation;

public interface IValidationRuleFixerApplier
{
    bool IsFixerRegistered(IValidationRule rule);
    void Apply(IValidationRule rule, IClonedRepository clonedRepository);
}