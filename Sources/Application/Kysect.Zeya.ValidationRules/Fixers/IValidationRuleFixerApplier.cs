using Kysect.Zeya.GithubIntegration;

namespace Kysect.Zeya.ValidationRules.Fixers;

public interface IValidationRuleFixerApplier
{
    bool IsFixerRegistered(string diagnosticCode);
    void Apply(string diagnosticCode, GithubRepositoryAccessor githubRepository);
}