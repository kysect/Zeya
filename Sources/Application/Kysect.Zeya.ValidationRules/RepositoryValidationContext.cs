using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules;

public record RepositoryValidationContext(GithubRepository GithubMetadata, IClonedRepository Repository, RepositoryDiagnosticCollector DiagnosticCollector);