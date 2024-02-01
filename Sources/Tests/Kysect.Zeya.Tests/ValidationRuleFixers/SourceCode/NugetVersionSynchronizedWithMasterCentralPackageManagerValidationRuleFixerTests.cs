using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.Tests.ValidationRules;
using Kysect.Zeya.ValidationRules.Fixers.SourceCode;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRuleFixers.SourceCode;

public class NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixerTests : ValidationRuleTestBase
{
    private readonly NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixer _fixer;

    public NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixerTests()
    {
        _fixer = new NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixer(FileSystem, RepositorySolutionAccessorFactory, Formatter, Logger);
    }

    [Fact]
    public void Fix_SolutionWithoutDirectoryPackageAndMasterFile_NoChanges()
    {
        var arguments = new NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments("Master.Directory.Package.props");

        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);

        _fixer.Fix(arguments, Repository);

        FileSystemAsserts
            .File(CurrentPath, SolutionItemNameConstants.DirectoryPackagesProps)
            .ShouldNotExists();
    }

    [Fact]
    public void Fix_PackageWithDifferentVersion_ReturnFixedVersion()
    {
        var masterFile = """
                         <Project>
                           <PropertyGroup>
                             <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                           </PropertyGroup>
                           <ItemGroup>
                             <PackageVersion Include="Package" Version="6.12.0" />
                           </ItemGroup>
                         </Project>
                         """;

        var inputDirectoryBuildPackagePropsContent = """
                                                     <Project>
                                                       <PropertyGroup>
                                                         <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                                       </PropertyGroup>
                                                       <ItemGroup>
                                                         <PackageVersion Include="Package" Version="1.2.3" />
                                                       </ItemGroup>
                                                     </Project>
                                                     """;

        var expected = """
                        <Project>
                          <PropertyGroup>
                            <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                          </PropertyGroup>
                          <ItemGroup>
                            <PackageVersion Include="Package" Version="6.12.0" />
                          </ItemGroup>
                        </Project>
                        """;

        var arguments = new NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments("Master.Directory.Package.props");

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryPackagesProps(inputDirectoryBuildPackagePropsContent)
            .Save(FileSystem, CurrentPath, Formatter);
        FileSystem.AddFile("Master.Directory.Package.props", new MockFileData(masterFile));

        _fixer.Fix(arguments, Repository);

        FileSystemAsserts
            .File(CurrentPath, SolutionItemNameConstants.DirectoryPackagesProps)
            .ShouldExists()
            .ShouldHaveContent(expected);
    }
}