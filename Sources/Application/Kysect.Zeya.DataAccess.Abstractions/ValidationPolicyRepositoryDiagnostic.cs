namespace Kysect.Zeya.DataAccess.Abstractions;

public class ValidationPolicyRepositoryDiagnostic(Guid validationPolicyRepositoryId, string ruleId, ValidationPolicyRepositoryDiagnosticSeverity severity)
{
    public Guid ValidationPolicyRepositoryId { get; init; } = validationPolicyRepositoryId;
    public string RuleId { get; init; } = ruleId;

    public ValidationPolicyRepositoryDiagnosticSeverity Severity { get; init; } = severity;
}