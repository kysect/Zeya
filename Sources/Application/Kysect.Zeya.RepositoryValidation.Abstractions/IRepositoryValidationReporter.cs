using Kysect.Zeya.RepositoryValidation.Abstractions.Models;

namespace Kysect.Zeya.RepositoryValidation.Abstractions;

public interface IRepositoryValidationReporter
{
    void Report(RepositoryValidationReport repositoryValidationReport);
}