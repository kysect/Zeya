using FluentAssertions;
using Kysect.ScenarioLib;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tests.Domain.RepositoryValidation.ProcessingActions.Validation;

public class RepositoryValidationProcessingActionTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly RepositoryValidationProcessingAction _validationProcessingAction;

    public RepositoryValidationProcessingActionTests()
    {
        _validationTestFixture = new ValidationTestFixture();

        ScenarioStepReflectionHandler scenarioStepReflectionHandler = ScenarioStepReflectionHandlerTestInstance.Create();
        ILogger<RepositoryValidationProcessingAction> logger = _validationTestFixture.GetLogger<RepositoryValidationProcessingAction>();
        _validationProcessingAction = new RepositoryValidationProcessingAction(scenarioStepReflectionHandler, new LoggerRepositoryValidationReporter(_validationTestFixture.GetLogger<LoggerRepositoryValidationReporter>()), logger);
    }

    [Fact]
    public void Validate_ForTwoRuleWithValidation_ReturnTwoDiagnostic()
    {
        var rules = new IValidationRule[]
        {
            new ArtifactsOutputEnabledValidationRule.Arguments(),
            new CentralPackageManagerEnabledValidationRule.Arguments()
        };

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        ILocalRepository localRepository = _validationTestFixture.CreateLocalRepository();

        var response = _validationProcessingAction.Process(localRepository, new RepositoryValidationProcessingActionRequest(rules));

        response
            .Messages
            .Should()
            .HaveCount(2);
    }

    [Fact]
    public void Validate_RuleThrowException_ReturnErrorMessageWithoutException()
    {
        var rules = new IValidationRule[]
        {
            new ThrowExceptionValidationRule.Arguments(),
        };

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        ILocalRepository localRepository = _validationTestFixture.CreateLocalRepository();

        var response = _validationProcessingAction.Process(localRepository, new RepositoryValidationProcessingActionRequest(rules));

        response
            .Messages
            .Where(m => m.Severity == RepositoryValidationSeverity.RuntimeError)
            .Should().HaveCount(1)
            .And.Subject.First().Message.Should().Be(ThrowExceptionValidationRule.Message);
    }
}