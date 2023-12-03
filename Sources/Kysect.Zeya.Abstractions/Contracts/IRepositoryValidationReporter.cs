using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IRepositoryValidationReporter
{
    void Report(RepositoryValidationReport repositoryValidationReport);
}