using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.Nuget;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.Nuget;

public class NugetMetadataHaveCorrectValueValidationRuleTests : ValidationRuleTestBase
{
    private readonly NugetMetadataHaveCorrectValueValidationRule _validationRule;

    public NugetMetadataHaveCorrectValueValidationRuleTests()
    {
        _validationRule = new NugetMetadataHaveCorrectValueValidationRule();
    }

    [Fact]
    public void Execute_NoDirectoryBuildPropsFile_ReturnDiagnosticAboutMissedDirectoryBuildProps()
    {
        var arguments = new NugetMetadataHaveCorrectValueValidationRule.Arguments([]);

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
        var arguments = new NugetMetadataHaveCorrectValueValidationRule.Arguments([]);
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
        var arguments = new NugetMetadataHaveCorrectValueValidationRule.Arguments(new Dictionary<string, string> { { "RequiredProperty", "true" } });
        var directoryBuildPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty());

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsFile)
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Directory.Build.props options has incorrect value or value is missed: RequiredProperty");
    }

    [Fact]
    public void Execute_RequiredPropertySpecifiedWithCorrectValue_ReturnNoDiagnostic()
    {
        var arguments = new NugetMetadataHaveCorrectValueValidationRule.Arguments(new Dictionary<string, string> { { "RequiredProperty", "true" } });
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

    [Fact]
    public void Execute_RequiredPropertySpecifiedWithDifferentValue_ReturnDiagnostic()
    {
        var arguments = new NugetMetadataHaveCorrectValueValidationRule.Arguments(new Dictionary<string, string> { { "RequiredProperty", "true" } });
        var directoryBuildPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty());
        directoryBuildPropsFile.File.Properties.SetProperty("RequiredProperty", "false");

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsFile)
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Directory.Build.props options has incorrect value or value is missed: RequiredProperty");
    }
}