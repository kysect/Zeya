namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

public record RepositoryValidationDiagnostic(string Code, string Message, RepositoryValidationSeverity Severity);