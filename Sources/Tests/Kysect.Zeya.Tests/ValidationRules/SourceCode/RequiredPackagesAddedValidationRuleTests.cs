using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;

namespace Kysect.Zeya.Tests.ValidationRules.SourceCode;

public class RequiredPackagesAddedValidationRuleTests : ValidationRuleTestBase
{
    private readonly RequiredPackagesAddedValidationRule _requiredPackagesAddedValidationRule;

    public RequiredPackagesAddedValidationRuleTests()
    {
        _requiredPackagesAddedValidationRule = new RequiredPackagesAddedValidationRule();
    }

    [Fact]
    public void Validate_EmptySolution_ReturnDiagnosticAboutMissedDirectoryBuildProps()
    {
        var arguments = new RequiredPackagesAddedValidationRule.Arguments([new ProjectPackageVersion("RequiredPackage", "1.0")]);
        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);

        _requiredPackagesAddedValidationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ValidationRuleMessages.DirectoryBuildPropsFileMissed);
    }

    [Fact]
    public void Validate_SolutionWithEmptyDirectoryBuildProps_ReturnDiagnosticAboutMissedPackage()
    {
        var arguments = new RequiredPackagesAddedValidationRule.Arguments([new ProjectPackageVersion("RequiredPackage", "1.0")]);
        new SolutionFileStructureBuilder("Solution")
            .AddFile([SolutionItemNameConstants.DirectoryBuildProps], string.Empty)
            .Save(FileSystem, CurrentPath, Formatter);

        _requiredPackagesAddedValidationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Package RequiredPackage is not add to Directory.Build.props.");
    }
}