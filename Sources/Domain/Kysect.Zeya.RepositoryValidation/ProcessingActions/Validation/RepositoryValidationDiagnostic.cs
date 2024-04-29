namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

public record RepositoryValidationDiagnostic(string Code, string Repository, string Message, RepositoryValidationSeverity Severity);