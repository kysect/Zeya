using FluentAssertions;
using Kysect.ScenarioLib;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.RepositoryValidation;

public class RepositoryValidatorTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly RepositoryValidationProcessingAction _validationProcessingAction;

    public RepositoryValidatorTests()
    {
        _validationTestFixture = new ValidationTestFixture();

        ScenarioStepReflectionHandler scenarioStepReflectionHandler = ScenarioStepReflectionHandlerTestInstance.Create();
        _validationProcessingAction = new RepositoryValidationProcessingAction(scenarioStepReflectionHandler, _validationTestFixture.GetLogger<RepositoryValidationProcessingAction>());
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

        RepositoryValidationReport repositoryValidationReport = _validationProcessingAction.Process(localRepository, new RepositoryValidationProcessingAction.Request(rules));

        repositoryValidationReport
            .Diagnostics
            .Should()
            .HaveCount(2);
    }
}