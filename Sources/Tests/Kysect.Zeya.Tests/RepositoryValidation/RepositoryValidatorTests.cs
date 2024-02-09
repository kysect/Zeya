using FluentAssertions;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.ScenarioLib;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.ValidationRules;
using Kysect.Zeya.ValidationRules.Abstractions;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;

namespace Kysect.Zeya.Tests.RepositoryValidation;

public class RepositoryValidatorTests : ValidationRuleTestBase
{
    private readonly RepositoryValidator _repositoryValidator;

    public RepositoryValidatorTests()
    {
        var scenarioStepReflectionHandler = new ScenarioStepReflectionHandler(new Dictionary<Type, ScenarioStepExecutorReflectionDecorator>()
        {
            {typeof(ArtifactsOutputEnabledValidationRule.Arguments), new ScenarioStepExecutorReflectionDecorator(new ArtifactsOutputEnabledValidationRule(RepositorySolutionAccessorFactory))},
            {typeof(CentralPackageManagerEnabledValidationRule.Arguments), new ScenarioStepExecutorReflectionDecorator(new CentralPackageManagerEnabledValidationRule(RepositorySolutionAccessorFactory))}
        });
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