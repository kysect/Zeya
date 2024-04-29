namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

public class RepositoryDiagnosticCollector
{
    private readonly List<RepositoryValidationDiagnostic> _diagnostics = new List<RepositoryValidationDiagnostic>();

    public void AddDiagnostic(string code, string message, RepositoryValidationSeverity severity)
    {
        _diagnostics.Add(new RepositoryValidationDiagnostic(code, message, severity));
    }

    public IReadOnlyCollection<RepositoryValidationDiagnostic> GetDiagnostics()
    {
        return _diagnostics;
    }
}