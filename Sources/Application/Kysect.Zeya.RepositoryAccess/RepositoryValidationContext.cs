using Kysect.Zeya.Abstractions.Contracts;

namespace Kysect.Zeya.RepositoryAccess;

public class RepositoryValidationContext
{
    public IClonedRepository Repository { get; init; }
    public RepositoryDiagnosticCollector DiagnosticCollector { get; init; }

    public RepositoryValidationContext(IClonedRepository repository, RepositoryDiagnosticCollector diagnosticCollector)
    {
        Repository = repository;
        DiagnosticCollector = diagnosticCollector;
    }
}