using FluentAssertions;
using Kysect.CommonLib.Logging;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tests.Domain.RepositoryValidation.ProcessingActions;

public class LoggerRepositoryValidationReporterTests
{
    private readonly StringBuilderLogger<LoggerRepositoryValidationReporter> _logger;
    private readonly LoggerRepositoryValidationReporter _loggerRepositoryValidationReporter;

    public LoggerRepositoryValidationReporterTests()
    {
        _logger = new StringBuilderLogger<LoggerRepositoryValidationReporter>(LogLevel.Trace);
        _loggerRepositoryValidationReporter = new LoggerRepositoryValidationReporter(_logger);
    }

    [Fact]
    public void Report_NoMessages_ReturnEmptyString()
    {
        var reporter = _loggerRepositoryValidationReporter;

        reporter.Report(new RepositoryValidationReport([]), "Repository");
        IReadOnlyCollection<string> logLines = _logger.Build();

        logLines.Should().HaveCount(0);
    }

    [Fact]
    public void Report_Error_ReturnTwoString()
    {
        var reporter = _loggerRepositoryValidationReporter;

        reporter.Report(new RepositoryValidationReport([new RepositoryProcessingMessage("ERR01", "Error message", RepositoryValidationSeverity.RuntimeError)]), "Repository");
        IReadOnlyCollection<string> logLines = _logger.Build();

        logLines.Should().HaveCount(2);
        logLines.ElementAt(0).Should().Be("Some analyzers finished with errors");
        logLines.ElementAt(1).Should().Be("\tRepository: [ERR01] Error message");
    }

    [Fact]
    public void Report_Diagnostic_ReturnString()
    {
        var reporter = _loggerRepositoryValidationReporter;

        reporter.Report(new RepositoryValidationReport([new RepositoryProcessingMessage("RUL01", "Diagnostic message", RepositoryValidationSeverity.Error)]), "Repository");
        IReadOnlyCollection<string> logLines = _logger.Build();

        logLines.Should().HaveCount(1);
        logLines.ElementAt(0).Should().Be("Repository: [RUL01] Diagnostic message");
    }
}