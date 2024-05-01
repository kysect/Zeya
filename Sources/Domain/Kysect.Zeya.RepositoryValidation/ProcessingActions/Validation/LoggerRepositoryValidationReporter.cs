using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Logging;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

public class LoggerRepositoryValidationReporter(ILogger<LoggerRepositoryValidationReporter> logger)
{
    public void Report(RepositoryValidationReport repositoryValidationReport, string repositoryName)
    {
        repositoryValidationReport.ThrowIfNull();

        if (repositoryValidationReport.RuntimeErrors.Any())
        {
            logger.LogWarning("Some analyzers finished with errors");
            foreach (RepositoryProcessingMessage diagnostic in repositoryValidationReport.RuntimeErrors)
                logger.LogTabWarning(1, $"{repositoryName}: [{diagnostic.Code}] {diagnostic.Message}");
        }

        foreach (var diagnostic in repositoryValidationReport.ValidationMessage)
        {
            logger.LogInformation($"{repositoryName}: [{diagnostic.Code}] {diagnostic.Message}");
        }
    }
}