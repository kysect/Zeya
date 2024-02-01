using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;

namespace Kysect.Zeya.Tests.ValidationRules.SourceCode;

public class ArtifactsOutputEnabledValidationRuleTests : ValidationRuleTestBase
{
    private readonly ArtifactsOutputEnabledValidationRule _validationRule;

    public ArtifactsOutputEnabledValidationRuleTests()
    {
        _validationRule = new ArtifactsOutputEnabledValidationRule(RepositorySolutionAccessorFactory);
    }

    [Fact]
    public void Validate_EmptySolution_ReturnDiagnosticAboutMissedDirectoryBuildProps()
    {
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();

        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ValidationRuleMessages.DirectoryBuildPropsFileMissed);
    }

    [Fact]
    public void Validate_EmptyDirectoryBuildProps_ReturnDiagnosticAboutMissedUseArtifactsOutput()
    {
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();

        new SolutionFileStructureBuilder("Solution")
            .AddFile([SolutionItemNameConstants.DirectoryBuildProps], string.Empty)
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ArtifactsOutputEnabledValidationRule.Arguments.UseArtifactsOutputOptionMustBeTrue);
    }

    [Fact]
    public void Validate_UseArtifactsOutputSetToFalse_ReturnDiagnosticAboutMissedUseArtifactsOutput()
    {
        var directoryBuildPropsContent = """
                                         <Project>
                                             <PropertyGroup>
                                                 <UseArtifactsOutput>false</UseArtifactsOutput>
                                             </PropertyGroup>
                                         </Project>
                                         """;
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();

        new SolutionFileStructureBuilder("Solution")
            .AddFile([SolutionItemNameConstants.DirectoryBuildProps], directoryBuildPropsContent)
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ArtifactsOutputEnabledValidationRule.Arguments.UseArtifactsOutputOptionMustBeTrue);
    }

    [Fact]
    public void Validate_UseArtifactsOutputSetToTrue_NoDiagnostic()
    {
        var directoryBuildPropsContent = """
                                         <Project>
                                             <PropertyGroup>
                                                 <UseArtifactsOutput>true</UseArtifactsOutput>
                                             </PropertyGroup>
                                         </Project>
                                         """;
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();

        new SolutionFileStructureBuilder("Solution")
            .AddFile([SolutionItemNameConstants.DirectoryBuildProps], directoryBuildPropsContent)
            .Save(FileSystem, CurrentPath, Formatter);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }
}