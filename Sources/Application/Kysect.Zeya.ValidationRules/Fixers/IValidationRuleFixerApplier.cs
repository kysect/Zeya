namespace Kysect.Zeya.ValidationRules.Fixers;

public interface IValidationRuleFixerApplier
{
    bool Apply(string diagnosticCode, string repositoryPath);
}