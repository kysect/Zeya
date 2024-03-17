using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.SourceCode;

public class CentralPackageManagerEnabledValidationRuleFixerTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly CentralPackageManagerEnabledValidationRuleFixer _fixer;

    public CentralPackageManagerEnabledValidationRuleFixerTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _fixer = new CentralPackageManagerEnabledValidationRuleFixer(_validationTestFixture.Formatter, _validationTestFixture.GetLogger<CentralPackageManagerEnabledValidationRuleFixer>());
    }

    [Fact]
    public void Fix_ProjectWithoutCpm_RemoveVersionsAndEnableCpm()
    {
        var originalProjectContent = """
                             <Project Sdk="Microsoft.NET.Sdk">
                               <PropertyGroup>
                                 <TargetFramework>net8.0</TargetFramework>
                               </PropertyGroup>
                               <ItemGroup>
                                 <PackageReference Include="FluentAssertions" Version="6.12.0" />
                               </ItemGroup>
                             </Project>
                             """;

        var expectedProjectContent = """
                             <Project Sdk="Microsoft.NET.Sdk">
                               <PropertyGroup>
                                 <TargetFramework>net8.0</TargetFramework>
                               </PropertyGroup>
                               <ItemGroup>
                                 <PackageReference Include="FluentAssertions" />
                               </ItemGroup>
                             </Project>
                             """;

        var expectedDotnetPackageContent = $"""
                                           <Project>
                                             <PropertyGroup>
                                               <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                             </PropertyGroup>
                                             <ItemGroup>
                                               <PackageVersion Include="FluentAssertions" Version="6.12.0" />
                                             </ItemGroup>
                                           </Project>
                                           """;

        const string projectName = "SampleProject";
        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName, originalProjectContent))
            .Save(_validationTestFixture.CurrentPath);

        LocalGithubRepository localGithubRepository = _validationTestFixture.CreateGithubRepository();
        _fixer.Fix(new CentralPackageManagerEnabledValidationRule.Arguments(), localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryPackagesProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDotnetPackageContent);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, projectName, $"{projectName}.csproj")
            .ShouldExists()
            .ShouldHaveContent(expectedProjectContent);
    }

    [Fact]
    public void Fix_BothProjectHaveReferenceToPackage_GeneratedDirectoryPackagePropsContainsDistinctPackages()
    {
        var originalProjectContent = """
                             <Project Sdk="Microsoft.NET.Sdk">
                               <PropertyGroup>
                                 <TargetFramework>net8.0</TargetFramework>
                               </PropertyGroup>
                               <ItemGroup>
                                 <PackageReference Include="FluentAssertions" Version="6.12.0" />
                               </ItemGroup>
                             </Project>
                             """;

        var expectedProjectContent = """
                             <Project Sdk="Microsoft.NET.Sdk">
                               <PropertyGroup>
                                 <TargetFramework>net8.0</TargetFramework>
                               </PropertyGroup>
                               <ItemGroup>
                                 <PackageReference Include="FluentAssertions" />
                               </ItemGroup>
                             </Project>
                             """;

        var expectedDotnetPackageContent = """
                                           <Project>
                                             <PropertyGroup>
                                               <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                             </PropertyGroup>
                                             <ItemGroup>
                                               <PackageVersion Include="FluentAssertions" Version="6.12.0" />
                                             </ItemGroup>
                                           </Project>
                                           """;

        const string projectName = "SampleProject";
        const string projectName2 = "SampleProject2";

        _validationTestFixture.SolutionFileStructureBuilderFactory.Create("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName, originalProjectContent))
            .AddProject(new ProjectFileStructureBuilder(projectName2, originalProjectContent))
            .Save(_validationTestFixture.CurrentPath);

        LocalGithubRepository localGithubRepository = _validationTestFixture.CreateGithubRepository();
        _fixer.Fix(new CentralPackageManagerEnabledValidationRule.Arguments(), localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryPackagesProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDotnetPackageContent);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, projectName, $"{projectName}.csproj")
            .ShouldExists()
            .ShouldHaveContent(expectedProjectContent);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, projectName2, $"{projectName2}.csproj")
            .ShouldExists()
            .ShouldHaveContent(expectedProjectContent);
    }
}