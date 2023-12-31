﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidation;

public class LoggerRepositoryValidationReporter : IRepositoryValidationReporter
{
    private readonly ILogger _logger;

    public LoggerRepositoryValidationReporter(ILogger logger)
    {
        _logger = logger;
    }

    public void Report(RepositoryValidationReport repositoryValidationReport)
    {
        repositoryValidationReport.ThrowIfNull();

        foreach (var diagnostic in repositoryValidationReport.Diagnostics)
        {
            // TODO: use severity
            _logger.LogWarning($"{diagnostic.Repository}: [{diagnostic.Code}] {diagnostic.Message}");
        }
    }
}