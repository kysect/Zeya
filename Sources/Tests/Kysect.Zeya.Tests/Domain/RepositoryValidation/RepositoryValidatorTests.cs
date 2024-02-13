using FluentAssertions;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.ScenarioLib;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Domain.ValidationRules;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.RepositoryValidation;

public class RepositoryValidatorTests : ValidationRuleTestBase
{
    private readonly RepositoryValidator _repositoryValidator;

    public RepositoryValidatorTests()
    {
        ScenarioStepReflectionHandler scenarioStepReflectionHandler = ScenarioStepReflectionHandlerTestInstance.Create();
        _repositoryValidator = new RepositoryValidator(TestLoggerProvider.GetLogger(), scenarioStepReflectionHandler);
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
            .Save(FileSystem, CurrentPath, Formatter);

        RepositoryValidationReport repositoryValidationReport = _repositoryValidator.Validate(Repository, rules);

        repositoryValidationReport
            .Diagnostics
            .Should()
            .HaveCount(2);
    }
}