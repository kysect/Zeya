using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules;

public class RepositoryValidationContext
{
    public static RepositoryValidationContext CreateForGitHub(GithubRepository githubMetadata, IClonedRepository repository)
    {
        githubMetadata.ThrowIfNull();

        return new RepositoryValidationContext(githubMetadata, repository, new RepositoryDiagnosticCollector(githubMetadata.FullName));
    }

    public static RepositoryValidationContext Create(IClonedRepository repository, RepositoryDiagnosticCollector diagnosticCollector)
    {
        return new RepositoryValidationContext(null, repository, diagnosticCollector);
    }

    private readonly GithubRepository? _githubRepository;

    public IClonedRepository Repository { get; init; }
    public RepositoryDiagnosticCollector DiagnosticCollector { get; init; }

    public RepositoryValidationContext(GithubRepository? githubMetadata, IClonedRepository repository, RepositoryDiagnosticCollector diagnosticCollector)
    {
        Repository = repository;
        DiagnosticCollector = diagnosticCollector;

        _githubRepository = githubMetadata;
    }

    public GithubRepository? TryGetGithubMetadata()
    {
        return _githubRepository;
    }
}