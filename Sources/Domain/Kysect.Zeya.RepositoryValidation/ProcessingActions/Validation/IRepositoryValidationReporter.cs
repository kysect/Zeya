namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

public interface IRepositoryValidationReporter
{
    void Report(RepositoryValidationReport repositoryValidationReport, string repositoryName);
}