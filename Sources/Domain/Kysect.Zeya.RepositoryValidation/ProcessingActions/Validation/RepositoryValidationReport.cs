using Kysect.CommonLib.BaseTypes.Extensions;
using System.Diagnostics.Contracts;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

public record RepositoryValidationReport(IReadOnlyCollection<RepositoryProcessingMessage> Diagnostics)
{
    public static RepositoryValidationReport Empty { get; } = new RepositoryValidationReport([]);

    public IReadOnlyCollection<RepositoryProcessingMessage> RuntimeErrors => Diagnostics.Where(d => d.Severity == RepositoryValidationSeverity.RuntimeError).ToList();
    public IReadOnlyCollection<RepositoryProcessingMessage> ValidationMessage => Diagnostics.Where(d => d.Severity != RepositoryValidationSeverity.RuntimeError).ToList();

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