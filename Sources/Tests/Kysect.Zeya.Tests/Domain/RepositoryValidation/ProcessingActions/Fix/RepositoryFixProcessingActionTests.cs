using FluentAssertions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Fix;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tests.Domain.RepositoryValidation.ProcessingActions.Fix;

public class RepositoryFixProcessingActionTests
{
    private readonly ValidationTestFixture _validationTestFixture;

    private readonly ILocalRepository _repository;
    private readonly Dictionary<Type, ValidationRuleFixerReflectionDecorator> _fixers;
    private readonly ValidationRuleFixerApplier _validationRuleFixerApplier;
    private readonly RepositoryFixProcessingAction _repositoryDiagnosticFixer;

    public RepositoryFixProcessingActionTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        ILogger<RepositoryFixProcessingAction> logger = _validationTestFixture.GetLogger<RepositoryFixProcessingAction>();
        _repository = _validationTestFixture.CreateLocalRepository();

        _fixers = new Dictionary<Type, ValidationRuleFixerReflectionDecorator>();
        _validationRuleFixerApplier = new ValidationRuleFixerApplier(_fixers);
        _repositoryDiagnosticFixer = new RepositoryFixProcessingAction(_validationRuleFixerApplier, logger);
    }

    [Fact]
    public void Fix_ForEmptyReport_ReturnNoFixedRules()
    {
        var report = RepositoryValidationReport.Empty;
        var request = new RepositoryFixProcessingAction.Request(Array.Empty<IValidationRule>(), report.GetAllDiagnosticRuleCodes());

        IReadOnlyCollection<IValidationRule> fixedRules = _repositoryDiagnosticFixer.Process(_repository, request).FixedRules;

        fixedRules.Should().BeEmpty();
    }

    [Fact]
    public void Fix_DiagnosticWithoutRule_ThrowException()
    {
        var repositoryValidationReport = new RepositoryValidationReport([new RepositoryValidationDiagnostic("CODE0001", "Some error", RepositoryValidationSeverity.Warning)]);
        var request = new RepositoryFixProcessingAction.Request(Array.Empty<IValidationRule>(), repositoryValidationReport.GetAllDiagnosticRuleCodes());

        _repositoryDiagnosticFixer
            .Invoking(f => f.Process(_repository, request))
            .Should()
            .Throw<DotnetProjectSystemException>()
            .WithMessage("Rule CODE0001 was not found");
    }

    [Fact]
    public void Fix_DiagnosticWithoutFixer_ReturnNoFixes()
    {
        var fakeValidationRule = new FakeValidationRule();
        IValidationRule[] validationRules = [fakeValidationRule];
        var repositoryValidationReport = new RepositoryValidationReport([new RepositoryValidationDiagnostic(fakeValidationRule.DiagnosticCode, "Some error", RepositoryValidationSeverity.Warning)]);
        var request = new RepositoryFixProcessingAction.Request(validationRules, repositoryValidationReport.GetAllDiagnosticRuleCodes());

        IReadOnlyCollection<IValidationRule> fixedRules = _repositoryDiagnosticFixer.Process(_repository, request).FixedRules;

        fixedRules.Should().BeEmpty();
    }

    [Fact]
    public void Fix_DiagnosticWithFixer_ReturnFixedRule()
    {
        var fakeValidationRule = new FakeValidationRule();
        var fakeValidationRuleFixer = new FakeValidationRuleFixer();
        IValidationRule[] validationRules = [fakeValidationRule];

        _fixers[fakeValidationRule.GetType()] = new ValidationRuleFixerReflectionDecorator(fakeValidationRuleFixer);
        var repositoryValidationReport = new RepositoryValidationReport([new RepositoryValidationDiagnostic(fakeValidationRule.DiagnosticCode, "Some error", RepositoryValidationSeverity.Warning)]);
        var request = new RepositoryFixProcessingAction.Request(validationRules, repositoryValidationReport.GetAllDiagnosticRuleCodes());

        IReadOnlyCollection<IValidationRule> fixedRules = _repositoryDiagnosticFixer.Process(_repository, request).FixedRules;

        fixedRules.Should().BeEquivalentTo([fakeValidationRule]);
        fakeValidationRuleFixer.FixCalls.Should().Be(1);
    }

    [Fact]
    public void Fix_TwoDiagnosticForOneRule_CallFixerOnce()
    {
        var fakeValidationRule = new FakeValidationRule();
        var fakeValidationRuleFixer = new FakeValidationRuleFixer();
        IValidationRule[] validationRules = [fakeValidationRule];

        _fixers[fakeValidationRule.GetType()] = new ValidationRuleFixerReflectionDecorator(fakeValidationRuleFixer);
        var repositoryValidationReport = new RepositoryValidationReport(
        [
            new RepositoryValidationDiagnostic(fakeValidationRule.DiagnosticCode, "Some error", RepositoryValidationSeverity.Warning),
            new RepositoryValidationDiagnostic(fakeValidationRule.DiagnosticCode, "Some error", RepositoryValidationSeverity.Warning)
        ]);
        var request = new RepositoryFixProcessingAction.Request(validationRules, repositoryValidationReport.GetAllDiagnosticRuleCodes());

        IReadOnlyCollection<IValidationRule> fixedRules = _repositoryDiagnosticFixer.Process(_repository, request).FixedRules;

        fixedRules.Should().BeEquivalentTo([fakeValidationRule]);
        fakeValidationRuleFixer.FixCalls.Should().Be(1);
    }
}