using Kysect.Zeya.LocalRepositoryAccess;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.Fix;

public interface IValidationRuleFixerApplier
{
    bool IsFixerRegistered(IValidationRule rule);
    void Apply(IValidationRule rule, ILocalRepository localRepository);
}