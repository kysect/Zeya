namespace Kysect.Zeya.Abstractions.Contracts;

public interface IValidationRuleFixer
{
}

public interface IValidationRuleFixer<TRules> : IValidationRuleFixer
    where TRules : IValidationRule
{
    void Fix(TRules rule, IClonedRepository clonedRepository);
}