using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetSlnGenerator.Builders;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Fixers.SourceCode;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRuleFixers.SourceCode;

public class CentralPackageManagerEnabledValidationRuleFixerTests
{
    private CentralPackageManagerEnabledValidationRuleFixer _fixer;
    private MockFileSystem _fileSystem;
    private ILogger _logger;

    [SetUp]
    public void Setup()
    {

        _logger = DefaultLoggerConfiguration.CreateConsoleLogger();
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        var dotnetSolutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, _logger);
        _fixer = new CentralPackageManagerEnabledValidationRuleFixer(dotnetSolutionModifierFactory, new RepositorySolutionAccessorFactory(new SolutionFileParser(_logger), _fileSystem), _logger);
    }

    [Test]
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

        string currentPath = _fileSystem.Path.GetFullPath(".");
        const string projectName = "SampleProject";
        var solutionBuilder = new DotnetSolutionBuilder("Solution")
            .AddProject(new DotnetProjectBuilder(projectName, originalProjectContent));
        solutionBuilder.Save(_fileSystem, currentPath);

        var githubRepositoryAccessorFactory = new GithubRepositoryAccessorFactory(new FakePathFormatStrategy(currentPath), _fileSystem);
        var repositoryAccessor = githubRepositoryAccessorFactory.Create(new GithubRepository("owner", "name"));
        _fixer.Fix(new CentralPackageManagerEnabledValidationRule.Arguments(), repositoryAccessor);

        _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(currentPath, projectName, $"{projectName}.csproj")).Should().Be(expectedProjectContent);
        _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(currentPath, ValidationConstants.DirectoryPackagePropsFileName)).Should().Be(expectedDotnetPackageContent);
    }

    [Test]
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

        // TODO: implement formatting rule
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

        string currentPath = _fileSystem.Path.GetFullPath(".");
        const string projectName = "SampleProject";
        const string projectName2 = "SampleProject2";

        new DotnetSolutionBuilder("Solution")
            .AddProject(new DotnetProjectBuilder(projectName, originalProjectContent))
            .AddProject(new DotnetProjectBuilder(projectName2, originalProjectContent))
            .Save(_fileSystem, currentPath);

        var githubRepositoryAccessorFactory = new GithubRepositoryAccessorFactory(new FakePathFormatStrategy(currentPath), _fileSystem);
        var repositoryAccessor = githubRepositoryAccessorFactory.Create(new GithubRepository("owner", "name"));
        _fixer.Fix(new CentralPackageManagerEnabledValidationRule.Arguments(), repositoryAccessor);

        _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(currentPath, projectName, $"{projectName}.csproj")).Should().Be(expectedProjectContent);
        _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(currentPath, projectName2, $"{projectName2}.csproj")).Should().Be(expectedProjectContent);
        _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(currentPath, ValidationConstants.DirectoryPackagePropsFileName)).Should().Be(expectedDotnetPackageContent);
    }
}