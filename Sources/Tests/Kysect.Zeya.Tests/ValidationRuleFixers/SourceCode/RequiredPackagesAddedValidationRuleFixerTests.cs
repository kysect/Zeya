using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.Tests.ValidationRules;
using Kysect.Zeya.ValidationRules.Fixers.SourceCode;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;

namespace Kysect.Zeya.Tests.ValidationRuleFixers.SourceCode;

public class RequiredPackagesAddedValidationRuleFixerTests : ValidationRuleTestBase
{
    private readonly RequiredPackagesAddedValidationRuleFixer _fixer;

    public RequiredPackagesAddedValidationRuleFixerTests()
    {
        _fixer = new RequiredPackagesAddedValidationRuleFixer(Formatter, Logger);
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
        new SolutionFileStructureBuilder("Solution")
            .Save(FileSystem, CurrentPath, Formatter);

        _fixer.Fix(arguments, Repository);

        FileSystemAsserts
            .File(CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDirectoryBuildPropsFile);

        FileSystemAsserts
            .File(CurrentPath, SolutionItemNameConstants.DirectoryPackagesProps)
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

        var projectPath = FileSystem.Path.Combine(CurrentPath, projectName, $"{projectName}.csproj");
        var arguments = new RequiredPackagesAddedValidationRule.Arguments([new ProjectPackageVersion("RequiredPackage", "1.0")]);
        new SolutionFileStructureBuilder("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName, projectFileContent))
            .Save(FileSystem, CurrentPath, Formatter);

        _fixer.Fix(arguments, Repository);

        FileSystemAsserts
            .File(CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDirectoryBuildPropsFile);

        FileSystemAsserts
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
        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryBuildProps(sourceDirectoryBuildPropsFile)
            .Save(FileSystem, CurrentPath, Formatter);

        _fixer.Fix(arguments, Repository);

        FileSystemAsserts
            .File(CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDirectoryBuildPropsFile);
    }
}