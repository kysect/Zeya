using Kysect.Zeya.LocalRepositoryAccess;

namespace Kysect.Zeya.RepositoryValidation.Abstractions;

public class RepositoryValidationContext
{
    public ILocalRepository Repository { get; init; }
    public RepositoryDiagnosticCollector DiagnosticCollector { get; init; }

    public RepositoryValidationContext(ILocalRepository repository, RepositoryDiagnosticCollector diagnosticCollector)
    {
        Repository = repository;
        DiagnosticCollector = diagnosticCollector;
    }
}