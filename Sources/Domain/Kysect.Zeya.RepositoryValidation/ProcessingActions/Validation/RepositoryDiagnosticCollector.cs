namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

public class RepositoryDiagnosticCollector
{
    private readonly List<RepositoryProcessingMessage> _diagnostics = new List<RepositoryProcessingMessage>();

    public void AddDiagnostic(string code, string message, RepositoryValidationSeverity severity)
    {
        _diagnostics.Add(new RepositoryProcessingMessage(code, message, severity));
    }

    public IReadOnlyCollection<RepositoryProcessingMessage> GetDiagnostics()
    {
        return _diagnostics;
    }
}