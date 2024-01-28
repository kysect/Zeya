using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules;

public class RepositoryDiagnosticCollector
{
    private readonly List<RepositoryValidationDiagnostic> _diagnostics = new List<RepositoryValidationDiagnostic>();
    private readonly List<RepositoryValidationDiagnostic> _runtimeErrors = new List<RepositoryValidationDiagnostic>();
    private readonly string _repository;

    public RepositoryDiagnosticCollector(string repository)
    {
        _repository = repository;
    }

    public void AddDiagnostic(string code, string message, RepositoryValidationSeverity severity)
    {
        _diagnostics.Add(new RepositoryValidationDiagnostic(code, _repository, message, severity));
    }

    public void AddRuntimeError(string code, string message, RepositoryValidationSeverity severity = RepositoryValidationSeverity.Error)
    {
        _runtimeErrors.Add(new RepositoryValidationDiagnostic(code, _repository, message, severity));
    }

    public IReadOnlyCollection<RepositoryValidationDiagnostic> GetDiagnostics()
    {
        return _diagnostics;
    }

    public IReadOnlyCollection<RepositoryValidationDiagnostic> GetRuntimeErrors()
    {
        return _runtimeErrors;
    }
}