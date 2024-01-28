using Kysect.CommonLib.BaseTypes.Extensions;
using System.Diagnostics.Contracts;

namespace Kysect.Zeya.Abstractions.Models;

public record RepositoryValidationReport(IReadOnlyCollection<RepositoryValidationDiagnostic> Diagnostics, IReadOnlyCollection<RepositoryValidationDiagnostic> RuntimeErrors)
{
    public static RepositoryValidationReport Empty { get; } = new RepositoryValidationReport([], []);

    [Pure]
    public RepositoryValidationReport Compose(RepositoryValidationReport other)
    {
        other.ThrowIfNull();

        return new RepositoryValidationReport(Diagnostics.Concat(other.Diagnostics).ToList(), RuntimeErrors.Concat(other.RuntimeErrors).ToList());
    }
}