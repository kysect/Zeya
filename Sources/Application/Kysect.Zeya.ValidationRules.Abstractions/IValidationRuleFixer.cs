using Kysect.Zeya.LocalRepositoryAccess;

namespace Kysect.Zeya.ValidationRules.Abstractions;

public interface IValidationRuleFixer
{
}

public interface IValidationRuleFixer<TRules> : IValidationRuleFixer
    where TRules : IValidationRule
{
    void Fix(TRules rule, ILocalRepository localRepository);
}