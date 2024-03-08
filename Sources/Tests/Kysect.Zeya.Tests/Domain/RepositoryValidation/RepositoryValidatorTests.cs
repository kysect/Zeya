using FluentAssertions;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.ScenarioLib;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.RepositoryValidation;

public class RepositoryValidatorTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly RepositoryValidator _repositoryValidator;

    public RepositoryValidatorTests()
    {
        _validationTestFixture = new ValidationTestFixture();

        ScenarioStepReflectionHandler scenarioStepReflectionHandler = ScenarioStepReflectionHandlerTestInstance.Create();
        _repositoryValidator = new RepositoryValidator(scenarioStepReflectionHandler, _validationTestFixture.GetLogger<RepositoryValidator>());
    }

    [Fact]
    public void Validate_ForTwoRuleWithValidation_ReturnTwoDiagnostic()
    {
        var rules = new IValidationRule[]
        {
            new ArtifactsOutputEnabledValidationRule.Arguments(),
            new CentralPackageManagerEnabledValidationRule.Arguments()
        };

        new SolutionFileStructureBuilder("Solution")
            .Save(_validationTestFixture.FileSystem, _validationTestFixture.CurrentPath, _validationTestFixture.Formatter);
        ILocalRepository localRepository = _validationTestFixture.RepositoryProvider.GetLocalRepository(_validationTestFixture.CurrentPath);

        RepositoryValidationReport repositoryValidationReport = _repositoryValidator.Validate(localRepository, rules);

        repositoryValidationReport
            .Diagnostics
            .Should()
            .HaveCount(2);
    }
}