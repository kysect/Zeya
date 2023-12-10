using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetSlnGenerator;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Fixers.SourceCode;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tests.ValidationRuleFixers;

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
        _fixer = new CentralPackageManagerEnabledValidationRuleFixer(_fileSystem, _logger);
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
                                           {'\t'}{'\t'}<PackageVersion Include="FluentAssertions" Version="6.12.0" />
                                             </ItemGroup>
                                           </Project>
                                           """;

        string currentPath = _fileSystem.Path.GetFullPath(".");
        const string projectName = "SampleProject";
        var solutionBuilder = new DotnetSolutionBuilder("Solution")
            .AddProject(new DotnetProjectBuilder(projectName, originalProjectContent));
        solutionBuilder.Save(_fileSystem, currentPath);

        var repositoryAccessor = new GithubRepositoryAccessor(new GithubRepository("owner", "name"), new FakePathFormatStrategy(currentPath), _fileSystem);
        _fixer.Fix(new CentralPackageManagerEnabledValidationRule.Arguments(), repositoryAccessor);

        _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(currentPath, projectName, $"{projectName}.csproj")).Should().Be(expectedProjectContent);
        _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(currentPath, ValidationConstants.DirectoryPackagePropsFileName)).Should().Be(expectedDotnetPackageContent);
    }
}