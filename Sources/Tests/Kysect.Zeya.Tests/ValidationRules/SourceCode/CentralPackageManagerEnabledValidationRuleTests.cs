using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;

namespace Kysect.Zeya.Tests.ValidationRules.SourceCode;

public class CentralPackageManagerEnabledValidationRuleTests : ValidationRuleTestBase
{
    private readonly CentralPackageManagerEnabledValidationRule _validationRule;

    public CentralPackageManagerEnabledValidationRuleTests()
    {
        _validationRule = new CentralPackageManagerEnabledValidationRule(RepositorySolutionAccessorFactory);
    }

    [Fact]
    public void Validate_EmptySolution_ReturnDiagnosticAboutDisabledArtifactOutput()
    {
        var arguments = new CentralPackageManagerEnabledValidationRule.Arguments();

        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, CentralPackageManagerEnabledValidationRule.Arguments.CentralPackageManagementDisabledMessage);
    }

    [Fact]
    public void Validate_ArtifactOutputEnabled_NoDiagnostics()
    {
        var arguments = new CentralPackageManagerEnabledValidationRule.Arguments();
        var directoryBuildPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty());
        directoryBuildPropsFile.File.Properties.AddProperty("UseArtifactsOutput", "true");

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsFile.File.ToXmlString(Formatter))
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }
}