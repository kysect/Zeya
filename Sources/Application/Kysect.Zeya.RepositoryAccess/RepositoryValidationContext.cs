using Kysect.Zeya.GitIntegration.Abstraction;

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