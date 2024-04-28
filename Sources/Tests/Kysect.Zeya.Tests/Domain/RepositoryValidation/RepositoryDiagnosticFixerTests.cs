using FluentAssertions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tests.Domain.RepositoryValidation;

public class RepositoryDiagnosticFixerTests
{
    private readonly ValidationTestFixture _validationTestFixture;

    private readonly ILogger<RepositoryFixProcessingAction> _logger;
    private readonly ILocalRepository _repository;

    public RepositoryDiagnosticFixerTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _logger = _validationTestFixture.GetLogger<RepositoryFixProcessingAction>();
        _repository = _validationTestFixture.CreateLocalRepository();
    }

    [Fact]
    public void Fix_ForEmptyReport_ReturnNoFixedRules()
    {
        var fixers = new Dictionary<Type, ValidationRuleFixerReflectionDecorator>();
        var validationRuleFixerApplier = new ValidationRuleFixerApplier(fixers);
        var repositoryDiagnosticFixer = new RepositoryFixProcessingAction(validationRuleFixerApplier, _logger);

        RepositoryValidationReport report = RepositoryValidationReport.Empty;
        IReadOnlyCollection<IValidationRule> fixedRules = repositoryDiagnosticFixer.Process(_repository, new RepositoryFixProcessingAction.Request(Array.Empty<IValidationRule>(), report.GetAllDiagnosticRuleCodes())).FixedRules;

        fixedRules.Should().BeEmpty();
    }

    [Fact]
    public void Fix_DiagnosticWithoutRule_ThrowException()
    {
        var fixers = new Dictionary<Type, ValidationRuleFixerReflectionDecorator>();
        var validationRuleFixerApplier = new ValidationRuleFixerApplier(fixers);
        var repositoryDiagnosticFixer = new RepositoryFixProcessingAction(validationRuleFixerApplier, _logger);
        var repositoryValidationReport = new RepositoryValidationReport([new RepositoryValidationDiagnostic("CODE0001", "Repository", "Some error", RepositoryValidationSeverity.Warning)], []);

        var exception = Assert.Throws<DotnetProjectSystemException>(() =>
        {
            repositoryDiagnosticFixer.Process(_repository, new RepositoryFixProcessingAction.Request(Array.Empty<IValidationRule>(), repositoryValidationReport.GetAllDiagnosticRuleCodes()));
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
        var repositoryDiagnosticFixer = new RepositoryFixProcessingAction(validationRuleFixerApplier, _logger);
        var repositoryValidationReport = new RepositoryValidationReport([new RepositoryValidationDiagnostic(fakeValidationRule.DiagnosticCode, "Repository", "Some error", RepositoryValidationSeverity.Warning)], []);

        IReadOnlyCollection<IValidationRule> fixedRules = repositoryDiagnosticFixer.Process(_repository, new RepositoryFixProcessingAction.Request(validationRules, repositoryValidationReport.GetAllDiagnosticRuleCodes())).FixedRules;

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
        var repositoryDiagnosticFixer = new RepositoryFixProcessingAction(validationRuleFixerApplier, _logger);
        var repositoryValidationReport = new RepositoryValidationReport([new RepositoryValidationDiagnostic(fakeValidationRule.DiagnosticCode, "Repository", "Some error", RepositoryValidationSeverity.Warning)], []);

        IReadOnlyCollection<IValidationRule> fixedRules = repositoryDiagnosticFixer.Process(_repository, new RepositoryFixProcessingAction.Request(validationRules, repositoryValidationReport.GetAllDiagnosticRuleCodes())).FixedRules;

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
        var repositoryDiagnosticFixer = new RepositoryFixProcessingAction(validationRuleFixerApplier, _logger);
        var repositoryValidationReport = new RepositoryValidationReport(
            [
                new RepositoryValidationDiagnostic(fakeValidationRule.DiagnosticCode, "Repository", "Some error", RepositoryValidationSeverity.Warning),
                new RepositoryValidationDiagnostic(fakeValidationRule.DiagnosticCode, "Repository", "Some error", RepositoryValidationSeverity.Warning)
            ],
            []);

        IReadOnlyCollection<IValidationRule> fixedRules = repositoryDiagnosticFixer.Process(_repository, new RepositoryFixProcessingAction.Request(validationRules, repositoryValidationReport.GetAllDiagnosticRuleCodes())).FixedRules;

        fixedRules.Should().BeEquivalentTo([fakeValidationRule]);
        fakeValidationRuleFixer.FixCalls.Should().Be(1);
    }
}