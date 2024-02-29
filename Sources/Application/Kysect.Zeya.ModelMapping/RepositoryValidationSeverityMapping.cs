using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.RepositoryValidation;

namespace Kysect.Zeya.ModelMapping;

// TODO: Need type-safe cast
public static class RepositoryValidationSeverityMapping
{
    public static RepositoryValidationSeverity ToDomainModel(ValidationPolicyRepositoryDiagnosticSeverity model)
    {
        return (RepositoryValidationSeverity) model;
    }

    public static ValidationPolicyRepositoryDiagnosticSeverity ToApplicationModel(RepositoryValidationSeverity model)
    {
        return (ValidationPolicyRepositoryDiagnosticSeverity) model;
    }
}
