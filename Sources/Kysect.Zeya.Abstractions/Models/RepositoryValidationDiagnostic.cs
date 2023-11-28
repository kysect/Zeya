namespace Kysect.Zeya.Abstractions.Models;

public record RepositoryValidationDiagnostic(string Code, string Project, string Message, RepositoryValidationSeverity Severity);