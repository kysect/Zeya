using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.SourceCode;

public class RequiredPackagesAddedValidationRuleFixerTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly RequiredPackagesAddedValidationRuleFixer _fixer;

    public RequiredPackagesAddedValidationRuleFixerTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _fixer = new RequiredPackagesAddedValidationRuleFixer(_validationTestFixture.Formatter, _validationTestFixture.GetLogger<RequiredPackagesAddedValidationRuleFixer>());
    }

    [Fact]
    public void Fix_OnEmptyDirectoryBuildProps_CreateExpectedContent()
    {
        const string expectedDirectoryBuildPropsFile = """
                                                       <Project>
                                                         <ItemGroup>
                                                           <PackageReference Include="RequiredPackage" />
                                                           <PackageReference Include="Package2" />
                                                         </ItemGroup>
                                                       </Project>
                                                       """;

        const string expectedDirectoryPackagePropsFile = """
                                                         <Project>
                                                           <ItemGroup>
                                                             <PackageVersion Include="RequiredPackage" Version="1.0" />
                                                             <PackageVersion Include="Package2" Version="1.2" />
                                                           </ItemGroup>
                                                         </Project>
                                                         """;

        var arguments = new RequiredPackagesAddedValidationRule.Arguments([new ProjectPackageVersion("RequiredPackage", "1.0"), new ProjectPackageVersion("Package2", "1.2")]);
        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        LocalGithubRepository localGithubRepository = _validationTestFixture.CreateGithubRepository();
        _fixer.Fix(arguments, localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDirectoryBuildPropsFile);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryPackagesProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDirectoryPackagePropsFile);

    }

    [Fact]
    public void Fix_ProjectWithManuallyAddedPackage_MustBeWithoutReferenceAfterFix()
    {
        string projectName = "Project";

        const string expectedDirectoryBuildPropsFile = """
                                                       <Project>
                                                         <ItemGroup>
                                                           <PackageReference Include="RequiredPackage" />
                                                         </ItemGroup>
                                                       </Project>
                                                       """;

        var projectFileContent = """
                                 <Project Sdk="Microsoft.NET.Sdk">
                                   <PropertyGroup>
                                     <TargetFramework>net8.0</TargetFramework>
                                   </PropertyGroup>
                                   <ItemGroup>
                                     <PackageReference Include="RequiredPackage" />
                                   </ItemGroup>
                                 </Project>
                                 """;

        var expectedProjectFileContent = """
                                 <Project Sdk="Microsoft.NET.Sdk">
                                   <PropertyGroup>
                                     <TargetFramework>net8.0</TargetFramework>
                                   </PropertyGroup>
                                   <ItemGroup>
                                   </ItemGroup>
                                 </Project>
                                 """;

        var projectPath = _validationTestFixture.FileSystem.Path.Combine(_validationTestFixture.CurrentPath, projectName, $"{projectName}.csproj");
        var arguments = new RequiredPackagesAddedValidationRule.Arguments([new ProjectPackageVersion("RequiredPackage", "1.0")]);
        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName, projectFileContent))
            .Save(_validationTestFixture.CurrentPath);

        LocalGithubRepository localGithubRepository = _validationTestFixture.CreateGithubRepository();
        _fixer.Fix(arguments, localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDirectoryBuildPropsFile);

        _validationTestFixture.FileSystemAsserts
            .File(projectPath)
            .ShouldExists()
            .ShouldHaveContent(expectedProjectFileContent);
    }

    [Fact]
    public void Fix_OnePackageAlreadyAdded_AddOnlyMissedPackage()
    {
        const string expectedDirectoryBuildPropsFile = """
                                                       <Project>
                                                         <ItemGroup>
                                                           <PackageReference Include="RequiredPackage" />
                                                           <PackageReference Include="Package2" />
                                                         </ItemGroup>
                                                       </Project>
                                                       """;
        const string sourceDirectoryBuildPropsFile = """
                                                     <Project>
                                                       <ItemGroup>
                                                         <PackageReference Include="RequiredPackage" />
                                                       </ItemGroup>
                                                     </Project>
                                                     """;

        var arguments = new RequiredPackagesAddedValidationRule.Arguments([new ProjectPackageVersion("RequiredPackage", "1.0"), new ProjectPackageVersion("Package2", "1.0")]);
        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddDirectoryBuildProps(sourceDirectoryBuildPropsFile)
            .Save(_validationTestFixture.CurrentPath);

        LocalGithubRepository localGithubRepository = _validationTestFixture.CreateGithubRepository();
        _fixer.Fix(arguments, localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDirectoryBuildPropsFile);
    }
}