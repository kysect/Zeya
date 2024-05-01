using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.SourceCode;

public class RequiredPackagesAddedValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly RequiredPackagesAddedValidationRule _requiredPackagesAddedValidationRule;

    public RequiredPackagesAddedValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _requiredPackagesAddedValidationRule = new RequiredPackagesAddedValidationRule();
    }

    [Fact]
    public void Validate_EmptySolution_ReturnDiagnosticAboutMissedDirectoryBuildProps()
    {
        var arguments = new RequiredPackagesAddedValidationRule.Arguments([new ProjectPackageVersion("RequiredPackage", "1.0")]);
        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        _requiredPackagesAddedValidationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ValidationRuleMessages.DirectoryBuildPropsFileMissed);
    }

    [Fact]
    public void Validate_SolutionWithEmptyDirectoryBuildProps_ReturnDiagnosticAboutMissedPackage()
    {
        var arguments = new RequiredPackagesAddedValidationRule.Arguments([new ProjectPackageVersion("RequiredPackage", "1.0")]);
        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddDirectoryBuildProps(string.Empty)
            .Save(_validationTestFixture.CurrentPath);

        _requiredPackagesAddedValidationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Package RequiredPackage is not add to Directory.Build.props.");
    }
}