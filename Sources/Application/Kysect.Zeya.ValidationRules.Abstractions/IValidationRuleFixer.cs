using Kysect.Zeya.GitIntegration.Abstraction;

namespace Kysect.Zeya.ValidationRules.Abstractions;

public interface IValidationRuleFixer
{
}

public interface IValidationRuleFixer<TRules> : IValidationRuleFixer
    where TRules : IValidationRule
{
    void Fix(TRules rule, IClonedRepository clonedRepository);
}