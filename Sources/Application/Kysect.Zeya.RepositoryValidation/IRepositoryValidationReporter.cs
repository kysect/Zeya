using Kysect.Zeya.RepositoryValidation.Models;

namespace Kysect.Zeya.RepositoryValidation;

public interface IRepositoryValidationReporter
{
    void Report(RepositoryValidationReport repositoryValidationReport);
}