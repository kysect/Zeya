using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Logging;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

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

        if (repositoryValidationReport.RuntimeErrors.Any())
        {
            _logger.LogError("Some analyzers finished with errors");
            foreach (RepositoryValidationDiagnostic diagnostic in repositoryValidationReport.Diagnostics)
                _logger.LogTabError(1, $"{diagnostic.Repository}: [{diagnostic.Code}] {diagnostic.Message}");
        }

        foreach (var diagnostic in repositoryValidationReport.Diagnostics)
        {
            // TODO: use severity
            _logger.LogWarning($"{diagnostic.Repository}: [{diagnostic.Code}] {diagnostic.Message}");
        }
    }
}