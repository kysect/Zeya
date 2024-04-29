using FluentAssertions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

namespace Kysect.Zeya.Tests.Tools.Asserts;

public class RepositoryDiagnosticCollectorAsserts
{
    private readonly RepositoryDiagnosticCollector _collector = new();

    public RepositoryDiagnosticCollector GetCollector()
    {
        return _collector;
    }

    public RepositoryDiagnosticCollectorAsserts ShouldHaveDiagnosticCount(int count)
    {
        IReadOnlyCollection<RepositoryValidationDiagnostic> diagnostics = _collector.GetDiagnostics();
        diagnostics.Count.Should().Be(count, "Founded diagnostics: " + diagnostics.ToSingleString(d => d.Message));
        return this;
    }

    public RepositoryDiagnosticCollectorAsserts ShouldHaveDiagnostic(int index, string diagnosticCode, string message)
    {
        index.Should().BeGreaterThan(0);

        IReadOnlyCollection<RepositoryValidationDiagnostic> diagnostics = _collector.GetDiagnostics();
        if (diagnostics.Count < index)
            Assert.Fail($"Diagnostic does not contains {index} diagnostics, diagnostic count {diagnostics.Count}");

        RepositoryValidationDiagnostic diagnostic = diagnostics.ElementAt(index - 1);
        diagnostic.Code.Should().Be(diagnosticCode);
        diagnostic.Message.Should().Be(message);
        return this;
    }

    public RepositoryDiagnosticCollectorAsserts ShouldHaveErrorCount(int count)
    {
        _collector
            .GetDiagnostics()
            .Where(d => d.Severity == RepositoryValidationSeverity.RuntimeError)
            .Should()
            .HaveCount(count);
        return this;
    }

    public RepositoryDiagnosticCollectorAsserts ShouldHaveError(int index, string diagnosticCode, string message)
    {
        RepositoryValidationDiagnostic diagnostic = _collector
            .GetDiagnostics()
            .Where(d => d.Severity == RepositoryValidationSeverity.RuntimeError)
            .ElementAt(index - 1);
        diagnostic.Code.Should().Be(diagnosticCode);
        diagnostic.Message.Should().Be(message);
        return this;
    }
}