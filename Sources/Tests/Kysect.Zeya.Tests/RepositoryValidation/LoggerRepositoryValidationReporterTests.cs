using FluentAssertions;
using Kysect.CommonLib.Logging;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryValidation;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tests.RepositoryValidation;

public class LoggerRepositoryValidationReporterTests
{
    private readonly StringBuilderLogger _logger;

    public LoggerRepositoryValidationReporterTests()
    {
        _logger = new StringBuilderLogger(LogLevel.Trace);
    }

    [Fact]
    public void Report_NoMessages_ReturnEmptyString()
    {
        var reporter = new LoggerRepositoryValidationReporter(_logger);

        reporter.Report(new RepositoryValidationReport([], []));
        IReadOnlyCollection<string> logLines = _logger.Build();

        logLines.Should().HaveCount(0);
    }

    [Fact]
    public void Report_Error_ReturnTwoString()
    {
        var reporter = new LoggerRepositoryValidationReporter(_logger);

        reporter.Report(new RepositoryValidationReport([], [new RepositoryValidationDiagnostic("ERR01", "Repository", "Error message", RepositoryValidationSeverity.Error)]));
        IReadOnlyCollection<string> logLines = _logger.Build();

        logLines.Should().HaveCount(2);
        logLines.ElementAt(0).Should().Be("Some analyzers finished with errors");
        logLines.ElementAt(1).Should().Be("\tRepository: [ERR01] Error message");
    }

    [Fact]
    public void Report_Diagnostic_ReturnString()
    {
        var reporter = new LoggerRepositoryValidationReporter(_logger);

        reporter.Report(new RepositoryValidationReport([new RepositoryValidationDiagnostic("RUL01", "Repository", "Diagnostic message", RepositoryValidationSeverity.Error)], []));
        IReadOnlyCollection<string> logLines = _logger.Build();

        logLines.Should().HaveCount(1);
        logLines.ElementAt(0).Should().Be("Repository: [RUL01] Diagnostic message");
    }
}