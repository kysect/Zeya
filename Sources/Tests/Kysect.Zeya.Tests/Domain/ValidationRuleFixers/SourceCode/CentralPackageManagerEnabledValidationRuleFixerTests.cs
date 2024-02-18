﻿using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Domain.ValidationRules;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.SourceCode;

public class CentralPackageManagerEnabledValidationRuleFixerTests : ValidationRuleTestBase
{
    private readonly CentralPackageManagerEnabledValidationRuleFixer _fixer;

    public CentralPackageManagerEnabledValidationRuleFixerTests()
    {
        _fixer = new CentralPackageManagerEnabledValidationRuleFixer(Formatter, Logger);
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
        new SolutionFileStructureBuilder("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName, originalProjectContent))
            .Save(FileSystem, CurrentPath, Formatter);

        _fixer.Fix(new CentralPackageManagerEnabledValidationRule.Arguments(), Repository);

        FileSystemAsserts
            .File(CurrentPath, SolutionItemNameConstants.DirectoryPackagesProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDotnetPackageContent);

        FileSystemAsserts
            .File(CurrentPath, projectName, $"{projectName}.csproj")
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

        new SolutionFileStructureBuilder("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName, originalProjectContent))
            .AddProject(new ProjectFileStructureBuilder(projectName2, originalProjectContent))
            .Save(FileSystem, CurrentPath, Formatter);

        _fixer.Fix(new CentralPackageManagerEnabledValidationRule.Arguments(), Repository);

        FileSystemAsserts
            .File(CurrentPath, SolutionItemNameConstants.DirectoryPackagesProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDotnetPackageContent);

        FileSystemAsserts
            .File(CurrentPath, projectName, $"{projectName}.csproj")
            .ShouldExists()
            .ShouldHaveContent(expectedProjectContent);

        FileSystemAsserts
            .File(CurrentPath, projectName2, $"{projectName2}.csproj")
            .ShouldExists()
            .ShouldHaveContent(expectedProjectContent);
    }
}