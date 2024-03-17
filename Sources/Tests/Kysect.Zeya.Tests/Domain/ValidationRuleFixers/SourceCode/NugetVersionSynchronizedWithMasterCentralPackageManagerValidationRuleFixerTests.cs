using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.SourceCode;

public class NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixerTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixer _fixer;

    public NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixerTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _fixer = new NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixer(_validationTestFixture.FileSystem, _validationTestFixture.Formatter, _validationTestFixture.GetLogger<NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixer>());
    }

    [Fact]
    public void Fix_SolutionWithoutDirectoryPackageAndMasterFile_NoChanges()
    {
        var arguments = new NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments("Master.Directory.Package.props");

        _validationTestFixture.SolutionFileStructureBuilderFactory.Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        LocalGithubRepository localGithubRepository = _validationTestFixture.CreateGithubRepository();
        _fixer.Fix(arguments, localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryPackagesProps)
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

        _validationTestFixture.SolutionFileStructureBuilderFactory.Create("Solution")
            .AddDirectoryPackagesProps(inputDirectoryBuildPackagePropsContent)
            .Save(_validationTestFixture.CurrentPath);
        _validationTestFixture.FileSystem.AddFile("Master.Directory.Package.props", new MockFileData(masterFile));

        LocalGithubRepository localGithubRepository = _validationTestFixture.CreateGithubRepository();
        _fixer.Fix(arguments, localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryPackagesProps)
            .ShouldExists()
            .ShouldHaveContent(expected);
    }
}