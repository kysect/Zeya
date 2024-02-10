using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Logging;
using Kysect.Zeya.RepositoryValidation.Abstractions;
using Kysect.Zeya.RepositoryValidation.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Kysect.Zeya.RepositoryValidation;

public class LoggerRepositoryValidationReporter(ILogger logger) : IRepositoryValidationReporter
{
    public void Report(RepositoryValidationReport repositoryValidationReport)
    {
        repositoryValidationReport.ThrowIfNull();

        if (repositoryValidationReport.RuntimeErrors.Any())
        {
            logger.LogError("Some analyzers finished with errors");
            foreach (RepositoryValidationDiagnostic diagnostic in repositoryValidationReport.RuntimeErrors)
                logger.LogTabError(1, $"{diagnostic.Repository}: [{diagnostic.Code}] {diagnostic.Message}");
        }

        foreach (var diagnostic in repositoryValidationReport.Diagnostics)
        {
            // TODO: use severity
            logger.LogWarning($"{diagnostic.Repository}: [{diagnostic.Code}] {diagnostic.Message}");
        }
    }
}