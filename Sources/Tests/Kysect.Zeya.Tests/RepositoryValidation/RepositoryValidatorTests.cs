using FluentAssertions;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.ScenarioLib;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.ValidationRules;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;

namespace Kysect.Zeya.Tests.RepositoryValidation;

public class RepositoryValidatorTests : ValidationRuleTestBase
{
    private readonly RepositoryValidator _repositoryValidator;
    private readonly ClonedGithubRepositoryAccessor _clonedGithubRepositoryAccessor;

    public RepositoryValidatorTests()
    {
        var scenarioStepReflectionHandler = new ScenarioStepReflectionHandler(new Dictionary<Type, ScenarioStepExecutorReflectionDecorator>()
        {
            {typeof(ArtifactsOutputEnabledValidationRule.Arguments), new ScenarioStepExecutorReflectionDecorator(new ArtifactsOutputEnabledValidationRule(RepositorySolutionAccessorFactory))},
            {typeof(CentralPackageManagerEnabledValidationRule.Arguments), new ScenarioStepExecutorReflectionDecorator(new CentralPackageManagerEnabledValidationRule(RepositorySolutionAccessorFactory))}
        });
        _repositoryValidator = new RepositoryValidator(TestLoggerProvider.GetLogger(), scenarioStepReflectionHandler);
        _clonedGithubRepositoryAccessor = new ClonedGithubRepositoryAccessor(new GithubRepository("owner", "name"), CurrentPath, FileSystem);
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

        RepositoryValidationReport repositoryValidationReport = _repositoryValidator.Validate(_clonedGithubRepositoryAccessor, rules);

        repositoryValidationReport
            .Diagnostics
            .Should()
            .HaveCount(2);
    }
}