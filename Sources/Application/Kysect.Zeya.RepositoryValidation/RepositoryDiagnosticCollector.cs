using Kysect.Zeya.RepositoryValidation.Models;
using System.Collections.Generic;

namespace Kysect.Zeya.RepositoryValidation;

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