namespace Kysect.Zeya.Abstractions.Models;

public record RepositoryValidationDiagnostic(string Code, string Repository, string Message, RepositoryValidationSeverity Severity);