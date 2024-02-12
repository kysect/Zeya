namespace Kysect.Zeya.RepositoryValidation;

public record RepositoryValidationDiagnostic(string Code, string Repository, string Message, RepositoryValidationSeverity Severity);