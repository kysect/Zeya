using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya;

public class LoggerRepositoryValidationReporter : IRepositoryValidationReporter
{
    private readonly ILogger _logger;

    public LoggerRepositoryValidationReporter(ILogger logger)
    {
        _logger = logger;
    }

    public void Report(RepositoryValidationReport repositoryValidationReport)
    {
        foreach (var diagnostic in repositoryValidationReport.Diagnostics)
        {
            // TODO: use severity
            _logger.LogInformation($"{diagnostic.Project}: [{diagnostic.Code}] {diagnostic.Message}");
        }
    }
}