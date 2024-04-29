using Kysect.CommonLib.BaseTypes.Extensions;
using System.Diagnostics.Contracts;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

public record RepositoryValidationReport(IReadOnlyCollection<RepositoryValidationDiagnostic> Diagnostics, IReadOnlyCollection<RepositoryValidationDiagnostic> RuntimeErrors)
{
    public static RepositoryValidationReport Empty { get; } = new RepositoryValidationReport([], []);

    [Pure]
    public RepositoryValidationReport Compose(RepositoryValidationReport other)
    {
        other.ThrowIfNull();

        return new RepositoryValidationReport(Diagnostics.Concat(other.Diagnostics).ToList(), RuntimeErrors.Concat(other.RuntimeErrors).ToList());
    }

    public IReadOnlyCollection<string> GetAllDiagnosticRuleCodes()
    {
        return Diagnostics
            .Select(d => d.Code)
            .Distinct()
            .ToList();
    }
}