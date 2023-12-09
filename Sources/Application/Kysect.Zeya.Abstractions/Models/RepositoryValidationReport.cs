using System.Diagnostics.Contracts;

namespace Kysect.Zeya.Abstractions.Models;

public record RepositoryValidationReport(IReadOnlyCollection<RepositoryValidationDiagnostic> Diagnostics)
{
    public static RepositoryValidationReport Empty { get; } = new RepositoryValidationReport([]);

    [Pure]
    public RepositoryValidationReport Compose(RepositoryValidationReport other)
    {
        return new RepositoryValidationReport(Diagnostics.Concat(other.Diagnostics).ToList());
    }
}