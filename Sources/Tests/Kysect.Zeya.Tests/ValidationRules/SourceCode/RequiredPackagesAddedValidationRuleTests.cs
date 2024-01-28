using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;

namespace Kysect.Zeya.Tests.ValidationRules.SourceCode;

public class RequiredPackagesAddedValidationRuleTests : ValidationRuleTestBase
{
    private readonly RequiredPackagesAddedValidationRule _requiredPackagesAddedValidationRule;

    public RequiredPackagesAddedValidationRuleTests()
    {
        _requiredPackagesAddedValidationRule = new RequiredPackagesAddedValidationRule(SolutionAccessorFactory);
    }

    [Fact]
    public void Validate_EmptySolution_ReturnDiagnosticAboutMissedDirectoryBuildProps()
    {
        var arguments = new RequiredPackagesAddedValidationRule.Arguments(["RequiredPackage"]);
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
        var arguments = new RequiredPackagesAddedValidationRule.Arguments(["RequiredPackage"]);
        new SolutionFileStructureBuilder("Solution")
            .AddFile([ValidationConstants.DirectoryBuildPropsFileName], string.Empty)
            .Save(FileSystem, CurrentPath, Formatter);

        _requiredPackagesAddedValidationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Package RequiredPackage is not add to Directory.Build.props.");
    }
}