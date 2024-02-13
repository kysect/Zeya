using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.SourceCode;

public class NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleTests : ValidationRuleTestBase
{
    private readonly string _directoryPackageMasterPropsPath;
    private readonly NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule _requiredPackagesAddedValidationRule;
    private readonly NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments _arguments;

    public NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleTests()
    {
        _directoryPackageMasterPropsPath = "Directory.Package.Master.props";
        _arguments = new NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments(_directoryPackageMasterPropsPath);
        _requiredPackagesAddedValidationRule = new NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule(FileSystem);
    }

    [Fact]
    public void Execute_NoMasterFile_ReturnDiagnosticAboutMissedMasterFile()
    {
        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);

        _requiredPackagesAddedValidationRule.Execute(Context, _arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(1)
            .ShouldHaveError(1, _arguments.DiagnosticCode, "Master file Directory.Package.Master.props for checking CPM was not found.");
    }

    [Fact]
    public void Execute_NoDirectoryBuildPropsFile_ReturnDiagnosticAboutDirectoryBuildPropsFile()
    {
        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);
        FileSystem.AddFile(_directoryPackageMasterPropsPath, new MockFileData(string.Empty));

        _requiredPackagesAddedValidationRule.Execute(Context, _arguments);

        DiagnosticCollectorAsserts
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
        string directoryPackageFileContent = directoryPackagesPropsFile.File.ToXmlString(Formatter);

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryPackagesProps(directoryPackageFileContent)
            .Save(FileSystem, CurrentPath, Formatter);
        FileSystem.AddFile(_directoryPackageMasterPropsPath, new MockFileData(directoryPackageFileContent));

        _requiredPackagesAddedValidationRule.Execute(Context, _arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }

    [Fact]
    public void Execute_PackageNotSpecifiedInPropsFile_ReturnDiagnosticAboutMissedPackage()
    {
        var directoryPackagesPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.CreateEmpty());
        directoryPackagesPropsFile.SetCentralPackageManagement(true);
        directoryPackagesPropsFile.Versions.AddPackageVersion("Package1", "1.2.3");
        string masterFileContent = directoryPackagesPropsFile.File.ToXmlString(Formatter);

        directoryPackagesPropsFile.Versions.AddPackageVersion("Package2", "2.3.4");
        string directoryPackagePropsFileContent = directoryPackagesPropsFile.File.ToXmlString(Formatter);

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryPackagesProps(directoryPackagePropsFileContent)
            .Save(FileSystem, CurrentPath, Formatter);
        FileSystem.AddFile(_directoryPackageMasterPropsPath, new MockFileData(masterFileContent));

        _requiredPackagesAddedValidationRule.Execute(Context, _arguments);

        DiagnosticCollectorAsserts
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
        string masterFileContent = directoryPackagesPropsFile.File.ToXmlString(Formatter);

        directoryPackagesPropsFile.Versions.SetPackageVersion("Package1", "2.3.4");
        string directoryPackagePropsFileContent = directoryPackagesPropsFile.File.ToXmlString(Formatter);

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryPackagesProps(directoryPackagePropsFileContent)
            .Save(FileSystem, CurrentPath, Formatter);
        FileSystem.AddFile(_directoryPackageMasterPropsPath, new MockFileData(masterFileContent));

        _requiredPackagesAddedValidationRule.Execute(Context, _arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, _arguments.DiagnosticCode, "Some Nuget packages versions is not synchronized with master Directory.Package.props: Package1");
    }
}