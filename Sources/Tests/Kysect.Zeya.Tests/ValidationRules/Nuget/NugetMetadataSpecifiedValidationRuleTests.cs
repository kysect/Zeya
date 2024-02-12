using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Rules.Nuget;

namespace Kysect.Zeya.Tests.ValidationRules.Nuget;

public class NugetMetadataSpecifiedValidationRuleTests : ValidationRuleTestBase
{
    private readonly NugetMetadataSpecifiedValidationRule _validationRule;

    public NugetMetadataSpecifiedValidationRuleTests()
    {
        _validationRule = new NugetMetadataSpecifiedValidationRule();
    }

    [Fact]
    public void Execute_NoDirectoryBuildPropsFile_ReturnDiagnosticAboutMissedDirectoryBuildProps()
    {
        var arguments = new NugetMetadataSpecifiedValidationRule.Arguments([]);

        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ValidationRuleMessages.DirectoryBuildPropsFileMissed);
    }

    [Fact]
    public void Execute_NoRules_NoDiagnostics()
    {
        var arguments = new NugetMetadataSpecifiedValidationRule.Arguments([]);
        var directoryBuildPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty());

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsFile)
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(0);
    }

    [Fact]
    public void Execute_RequiredPropertyMissed_ReturnDiagnosticAboutMissedProperty()
    {
        var arguments = new NugetMetadataSpecifiedValidationRule.Arguments(["RequiredProperty"]);
        var directoryBuildPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty());

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsFile)
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Directory.Build.props file does not contains required option: RequiredProperty");
    }

    [Fact]
    public void Execute_RequiredPropertySpecified_ReturnNoDiagnostic()
    {
        var arguments = new NugetMetadataSpecifiedValidationRule.Arguments(["RequiredProperty"]);
        var directoryBuildPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty());
        directoryBuildPropsFile.File.Properties.SetProperty("RequiredProperty", "true");

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsFile)
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(0);
    }
}