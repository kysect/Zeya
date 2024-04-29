using Kysect.CommonLib.BaseTypes.Extensions;
using System.Diagnostics.Contracts;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

public record RepositoryValidationReport(IReadOnlyCollection<RepositoryValidationDiagnostic> Diagnostics)
{
    public static RepositoryValidationReport Empty { get; } = new RepositoryValidationReport([]);

    public IReadOnlyCollection<RepositoryValidationDiagnostic> RuntimeErrors => Diagnostics.Where(d => d.Severity == RepositoryValidationSeverity.RuntimeError).ToList();
    public IReadOnlyCollection<RepositoryValidationDiagnostic> ValidationMessage => Diagnostics.Where(d => d.Severity != RepositoryValidationSeverity.RuntimeError).ToList();

    [Pure]
    public RepositoryValidationReport Compose(RepositoryValidationReport other)
    {
        other.ThrowIfNull();

        return new RepositoryValidationReport(Diagnostics.Concat(other.Diagnostics).ToList());
    }

    public IReadOnlyCollection<string> GetAllDiagnosticRuleCodes()
    {
        return Diagnostics
            .Select(d => d.Code)
            .Distinct()
            .ToList();
    }
}