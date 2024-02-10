using FluentAssertions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.Models;

namespace Kysect.Zeya.Tests.Asserts;

public class RepositoryDiagnosticCollectorAsserts
{
    private readonly RepositoryDiagnosticCollector _collector;
    public RepositoryDiagnosticCollectorAsserts(string repository)
    {
        _collector = new RepositoryDiagnosticCollector(repository);
    }

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
        RepositoryValidationDiagnostic diagnostic = _collector.GetDiagnostics().ElementAt(index - 1);
        diagnostic.Code.Should().Be(diagnosticCode);
        diagnostic.Message.Should().Be(message);
        return this;
    }

    public RepositoryDiagnosticCollectorAsserts ShouldHaveErrorCount(int count)
    {
        _collector.GetRuntimeErrors().Count.Should().Be(count);
        return this;
    }

    public RepositoryDiagnosticCollectorAsserts ShouldHaveError(int index, string diagnosticCode, string message)
    {
        RepositoryValidationDiagnostic diagnostic = _collector.GetRuntimeErrors().ElementAt(index - 1);
        diagnostic.Code.Should().Be(diagnosticCode);
        diagnostic.Message.Should().Be(message);
        return this;
    }
}