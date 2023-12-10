using Kysect.Zeya.Abstractions.Contracts;

namespace Kysect.Zeya.ValidationRules.Fixers;

public interface IValidationRuleFixer
{
    string DiagnosticCode { get; }

    void Fix(IGithubRepositoryAccessor githubRepository);
}