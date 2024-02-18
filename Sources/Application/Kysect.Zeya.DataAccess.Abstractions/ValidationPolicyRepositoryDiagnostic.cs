namespace Kysect.Zeya.DataAccess.Abstractions;

public class ValidationPolicyRepositoryDiagnostic(Guid validationPolicyRepositoryId, string ruleId, string severity)
{
    public Guid ValidationPolicyRepositoryId { get; init; } = validationPolicyRepositoryId;
    public string RuleId { get; init; } = ruleId;

    // TODO: typed Severity
    public string Severity { get; init; } = severity;
}