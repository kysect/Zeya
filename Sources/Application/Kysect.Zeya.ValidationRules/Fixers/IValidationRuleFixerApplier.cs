using Kysect.Zeya.Abstractions.Contracts;

namespace Kysect.Zeya.ValidationRules.Fixers;

public interface IValidationRuleFixerApplier
{
    bool IsFixerRegistered(string diagnosticCode);
    void Apply(string diagnosticCode, IGithubRepositoryAccessor githubRepository);
}