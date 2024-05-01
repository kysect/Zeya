using FluentAssertions;
using Kysect.ScenarioLib;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;
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

        var response = _validationProcessingAction.Process(localRepository, new RepositoryValidationProcessingAction.Request(rules));

        response
            .Messages
            .Should()
            .HaveCount(2);
    }
}