using Kysect.Zeya.GithubIntegration;

namespace Kysect.Zeya.ValidationRules.Fixers;

public interface IValidationRuleFixer
{
    string DiagnosticCode { get; }

    void Fix(GithubRepositoryAccessor githubRepository);
}