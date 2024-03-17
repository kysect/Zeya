using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.SourceCode;

public class NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly string _directoryPackageMasterPropsPath;
    private readonly NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule _requiredPackagesAddedValidationRule;
    private readonly NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments _arguments;

    public NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _directoryPackageMasterPropsPath = "Directory.Package.Master.props";
        _arguments = new NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments(_directoryPackageMasterPropsPath);
        _requiredPackagesAddedValidationRule = new NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule(_validationTestFixture.FileSystem);
    }

    [Fact]
    public void Execute_NoMasterFile_ReturnDiagnosticAboutMissedMasterFile()
    {
        _validationTestFixture.SolutionFileStructureBuilderFactory.Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        _requiredPackagesAddedValidationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), _arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(1)
            .ShouldHaveError(1, _arguments.DiagnosticCode, "Master file Directory.Package.Master.props for checking CPM was not found.");
    }

    [Fact]
    public void Execute_NoDirectoryBuildPropsFile_ReturnDiagnosticAboutDirectoryBuildPropsFile()
    {
        _validationTestFixture.SolutionFileStructureBuilderFactory.Create("Solution")
            .Save(_validationTestFixture.CurrentPath);
        _validationTestFixture.FileSystem.AddFile(_directoryPackageMasterPropsPath, new MockFileData(string.Empty));

        _requiredPackagesAddedValidationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), _arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, _arguments.DiagnosticCode, NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments.DirectoryPackagePropsFileMissed);
    }

    [Fact]
    public void Execute_DirectoryBuildPropsFileSameWithMaster_ReturnNoDiagnostic()
    {
        var directoryPackagesPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.CreateEmpty());
        directoryPackagesPropsFile.SetCentralPackageManagement(true);
        directoryPackagesPropsFile.Versions.AddPackageVersion("Package1", "1.2.3");
        directoryPackagesPropsFile.Versions.AddPackageVersion("Package2", "2.3.4");
        string directoryPackageFileContent = directoryPackagesPropsFile.File.ToXmlString(_validationTestFixture.Formatter);

        _validationTestFixture.SolutionFileStructureBuilderFactory.Create("Solution")
            .AddDirectoryPackagesProps(directoryPackageFileContent)
            .Save(_validationTestFixture.CurrentPath);
        _validationTestFixture.FileSystem.AddFile(_directoryPackageMasterPropsPath, new MockFileData(directoryPackageFileContent));

        _requiredPackagesAddedValidationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), _arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }

    [Fact]
    public void Execute_PackageNotSpecifiedInPropsFile_ReturnDiagnosticAboutMissedPackage()
    {
        var directoryPackagesPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.CreateEmpty());
        directoryPackagesPropsFile.SetCentralPackageManagement(true);
        directoryPackagesPropsFile.Versions.AddPackageVersion("Package1", "1.2.3");
        string masterFileContent = directoryPackagesPropsFile.File.ToXmlString(_validationTestFixture.Formatter);

        directoryPackagesPropsFile.Versions.AddPackageVersion("Package2", "2.3.4");
        string directoryPackagePropsFileContent = directoryPackagesPropsFile.File.ToXmlString(_validationTestFixture.Formatter);

        _validationTestFixture.SolutionFileStructureBuilderFactory.Create("Solution")
            .AddDirectoryPackagesProps(directoryPackagePropsFileContent)
            .Save(_validationTestFixture.CurrentPath);
        _validationTestFixture.FileSystem.AddFile(_directoryPackageMasterPropsPath, new MockFileData(masterFileContent));

        _requiredPackagesAddedValidationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), _arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, _arguments.DiagnosticCode, "Some Nuget packages versions is not synchronized with master Directory.Package.props: Package2");
    }

    [Fact]
    public void Execute_DifferentVersionInPropsAndMasterFile_ReturnDiagnosticAboutDifferentVersions()
    {
        var directoryPackagesPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.CreateEmpty());
        directoryPackagesPropsFile.SetCentralPackageManagement(true);
        directoryPackagesPropsFile.Versions.AddPackageVersion("Package1", "1.2.3");
        string masterFileContent = directoryPackagesPropsFile.File.ToXmlString(_validationTestFixture.Formatter);

        directoryPackagesPropsFile.Versions.SetPackageVersion("Package1", "2.3.4");
        string directoryPackagePropsFileContent = directoryPackagesPropsFile.File.ToXmlString(_validationTestFixture.Formatter);

        _validationTestFixture.SolutionFileStructureBuilderFactory.Create("Solution")
            .AddDirectoryPackagesProps(directoryPackagePropsFileContent)
            .Save(_validationTestFixture.CurrentPath);
        _validationTestFixture.FileSystem.AddFile(_directoryPackageMasterPropsPath, new MockFileData(masterFileContent));

        _requiredPackagesAddedValidationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), _arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, _arguments.DiagnosticCode, "Some Nuget packages versions is not synchronized with master Directory.Package.props: Package1");
    }
}