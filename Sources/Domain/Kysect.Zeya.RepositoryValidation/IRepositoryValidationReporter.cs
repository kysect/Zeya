namespace Kysect.Zeya.RepositoryValidation;

public interface IRepositoryValidationReporter
{
    void Report(RepositoryValidationReport repositoryValidationReport);
}