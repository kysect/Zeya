﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Logging;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidation;

public class LoggerRepositoryValidationReporter(ILogger<LoggerRepositoryValidationReporter> logger) : IRepositoryValidationReporter
{
    public void Report(RepositoryValidationReport repositoryValidationReport)
    {
        repositoryValidationReport.ThrowIfNull();

        if (repositoryValidationReport.RuntimeErrors.Any())
        {
            logger.LogWarning("Some analyzers finished with errors");
            foreach (RepositoryValidationDiagnostic diagnostic in repositoryValidationReport.RuntimeErrors)
                logger.LogTabWarning(1, $"{diagnostic.Repository}: [{diagnostic.Code}] {diagnostic.Message}");
        }

        foreach (var diagnostic in repositoryValidationReport.Diagnostics)
        {
            logger.LogInformation($"{diagnostic.Repository}: [{diagnostic.Code}] {diagnostic.Message}");
        }
    }
}