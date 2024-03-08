using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.SourceCode;

public class CentralPackageManagerEnabledValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly CentralPackageManagerEnabledValidationRule _validationRule;

    public CentralPackageManagerEnabledValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _validationRule = new CentralPackageManagerEnabledValidationRule();
    }

    [Fact]
    public void Validate_EmptySolution_ReturnDiagnosticAboutDisabledArtifactOutput()
    {
        var arguments = new CentralPackageManagerEnabledValidationRule.Arguments();

        new SolutionFileStructureBuilder("Solution")
            .Save(_validationTestFixture.FileSystem, _validationTestFixture.CurrentPath, _validationTestFixture.Formatter);

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, CentralPackageManagerEnabledValidationRule.Arguments.CentralPackageManagementDisabledMessage);
    }

    [Fact]
    public void Validate_ArtifactOutputEnabled_NoDiagnostics()
    {
        var arguments = new CentralPackageManagerEnabledValidationRule.Arguments();
        var directoryBuildPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.CreateEmpty());
        directoryBuildPropsFile.SetCentralPackageManagement(true);

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryPackagesProps(directoryBuildPropsFile.File.ToXmlString(_validationTestFixture.Formatter))
            .Save(_validationTestFixture.FileSystem, _validationTestFixture.CurrentPath, _validationTestFixture.Formatter);

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }
}