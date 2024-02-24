using FluentAssertions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.RepositoryValidation;

public class RepositoryDiagnosticFixerTests
{
    private readonly ILogger<RepositoryDiagnosticFixer> _logger;
    private readonly MockFileSystem _fileSystem;
    private readonly string _currentPath;
    private readonly LocalRepository _repository;

    public RepositoryDiagnosticFixerTests()
    {
        _logger = TestLoggerProvider.GetLogger<RepositoryDiagnosticFixer>();
        _fileSystem = new MockFileSystem();
        _currentPath = _fileSystem.Path.GetFullPath(".");
        _repository = new LocalRepository(_currentPath, _fileSystem);
    }

    [Fact]
    public void Fix_ForEmptyReport_ReturnNoFixedRules()
    {
        var fixers = new Dictionary<Type, ValidationRuleFixerReflectionDecorator>();
        var validationRuleFixerApplier = new ValidationRuleFixerApplier(fixers);
        var repositoryDiagnosticFixer = new RepositoryDiagnosticFixer(validationRuleFixerApplier, _logger);

        IReadOnlyCollection<IValidationRule> fixedRules = repositoryDiagnosticFixer.Fix(RepositoryValidationReport.Empty, Array.Empty<IValidationRule>(), _repository);

        fixedRules.Should().BeEmpty();
    }

    [Fact]
    public void Fix_DiagnosticWithoutRule_ThrowException()
    {
        var fixers = new Dictionary<Type, ValidationRuleFixerReflectionDecorator>();
        var validationRuleFixerApplier = new ValidationRuleFixerApplier(fixers);
        var repositoryDiagnosticFixer = new RepositoryDiagnosticFixer(validationRuleFixerApplier, _logger);
        var repositoryValidationReport = new RepositoryValidationReport([new RepositoryValidationDiagnostic("CODE0001", "Repository", "Some error", RepositoryValidationSeverity.Warning)], []);

        var exception = Assert.Throws<DotnetProjectSystemException>(() =>
        {
            repositoryDiagnosticFixer.Fix(repositoryValidationReport, Array.Empty<IValidationRule>(), _repository);
        });

        exception.Message.Should().Be("Rule CODE0001 was not found");
    }

    [Fact]
    public void Fix_DiagnosticWithoutFixer_ReturnNoFixes()
    {
        var fakeValidationRule = new FakeValidationRule();
        IValidationRule[] validationRules = [fakeValidationRule];

        var fixers = new Dictionary<Type, ValidationRuleFixerReflectionDecorator>();
        var validationRuleFixerApplier = new ValidationRuleFixerApplier(fixers);
        var repositoryDiagnosticFixer = new RepositoryDiagnosticFixer(validationRuleFixerApplier, _logger);
        var repositoryValidationReport = new RepositoryValidationReport([new RepositoryValidationDiagnostic(fakeValidationRule.DiagnosticCode, "Repository", "Some error", RepositoryValidationSeverity.Warning)], []);

        IReadOnlyCollection<IValidationRule> fixedRules = repositoryDiagnosticFixer.Fix(repositoryValidationReport, validationRules, _repository);

        fixedRules.Should().BeEmpty();
    }

    [Fact]
    public void Fix_DiagnosticWithFixer_ReturnFixedRule()
    {
        var fakeValidationRule = new FakeValidationRule();
        var fakeValidationRuleFixer = new FakeValidationRuleFixer();
        IValidationRule[] validationRules = [fakeValidationRule];

        var fixers = new Dictionary<Type, ValidationRuleFixerReflectionDecorator>()
        {
            {fakeValidationRule.GetType(), new ValidationRuleFixerReflectionDecorator(fakeValidationRuleFixer)}
        };
        var validationRuleFixerApplier = new ValidationRuleFixerApplier(fixers);
        var repositoryDiagnosticFixer = new RepositoryDiagnosticFixer(validationRuleFixerApplier, _logger);
        var repositoryValidationReport = new RepositoryValidationReport([new RepositoryValidationDiagnostic(fakeValidationRule.DiagnosticCode, "Repository", "Some error", RepositoryValidationSeverity.Warning)], []);

        IReadOnlyCollection<IValidationRule> fixedRules = repositoryDiagnosticFixer.Fix(repositoryValidationReport, validationRules, _repository);

        fixedRules.Should().BeEquivalentTo([fakeValidationRule]);
        fakeValidationRuleFixer.FixCalls.Should().Be(1);
    }

    [Fact]
    public void Fix_TwoDiagnosticForOneRule_CallFixerOnce()
    {
        var fakeValidationRule = new FakeValidationRule();
        var fakeValidationRuleFixer = new FakeValidationRuleFixer();
        IValidationRule[] validationRules = [fakeValidationRule];

        var fixers = new Dictionary<Type, ValidationRuleFixerReflectionDecorator>()
        {
            {fakeValidationRule.GetType(), new ValidationRuleFixerReflectionDecorator(fakeValidationRuleFixer)}
        };
        var validationRuleFixerApplier = new ValidationRuleFixerApplier(fixers);
        var repositoryDiagnosticFixer = new RepositoryDiagnosticFixer(validationRuleFixerApplier, _logger);
        var repositoryValidationReport = new RepositoryValidationReport(
            [
                new RepositoryValidationDiagnostic(fakeValidationRule.DiagnosticCode, "Repository", "Some error", RepositoryValidationSeverity.Warning),
                new RepositoryValidationDiagnostic(fakeValidationRule.DiagnosticCode, "Repository", "Some error", RepositoryValidationSeverity.Warning)
            ],
            []);

        IReadOnlyCollection<IValidationRule> fixedRules = repositoryDiagnosticFixer.Fix(repositoryValidationReport, validationRules, _repository);

        fixedRules.Should().BeEquivalentTo([fakeValidationRule]);
        fakeValidationRuleFixer.FixCalls.Should().Be(1);
    }
}