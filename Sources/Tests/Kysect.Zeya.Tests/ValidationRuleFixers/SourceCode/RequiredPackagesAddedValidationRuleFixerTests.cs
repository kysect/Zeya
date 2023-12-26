using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetSlnGenerator.Builders;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Fixers.SourceCode;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRuleFixers.SourceCode;

public class RequiredPackagesAddedValidationRuleFixerTests
{
    private RequiredPackagesAddedValidationRuleFixer _requiredPackagesAddedValidationRuleFixer;
    private string _currentPath;
    private MockFileSystem _fileSystem;
    private ClonedRepository _clonedRepository;
    private RepositorySolutionAccessor _repositorySolutionAccessor;

    [SetUp]
    public void Setup()
    {
        ILogger logger = DefaultLoggerConfiguration.CreateConsoleLogger();
        _fileSystem = new MockFileSystem();
        _currentPath = _fileSystem.Path.GetFullPath(".");

        var repositorySolutionAccessorFactory = new RepositorySolutionAccessorFactory(new SolutionFileParser(logger), _fileSystem);
        _requiredPackagesAddedValidationRuleFixer
            = new RequiredPackagesAddedValidationRuleFixer(
                new DotnetSolutionModifierFactory(
                    _fileSystem,
                    logger),
                repositorySolutionAccessorFactory,
                logger);

        _clonedRepository = new ClonedRepository(_currentPath, _fileSystem);
        _repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(_clonedRepository);
    }

    [Test]
    public void Fix_OnEmptyDirectoryBuildProps_CreateExpectedContent()
    {
        const string expectedDirectoryBuildPropsFile = """
                                                       <Project>
                                                       	<ItemGroup>
                                                       		<PackageReference Include="RequiredPackage" />
                                                       	</ItemGroup>
                                                       </Project>
                                                       """;

        var arguments = new RequiredPackagesAddedValidationRule.Arguments(["RequiredPackage"]);
        new DotnetSolutionBuilder("Solution")
            .Save(_fileSystem, _currentPath);

        _requiredPackagesAddedValidationRuleFixer.Fix(
            arguments,
            _clonedRepository);

        _fileSystem.File.ReadAllText(_repositorySolutionAccessor.GetDirectoryBuildPropsPath()).Should().Be(expectedDirectoryBuildPropsFile);
    }

    [Test]
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

        var projectPath = _fileSystem.Path.Combine(_currentPath, projectName, $"{projectName}.csproj");
        var arguments = new RequiredPackagesAddedValidationRule.Arguments(["RequiredPackage"]);
        new DotnetSolutionBuilder("Solution")
            .AddProject(new DotnetProjectBuilder(projectName, projectFileContent))
            .Save(_fileSystem, _currentPath);

        _requiredPackagesAddedValidationRuleFixer.Fix(
            arguments,
            _clonedRepository);

        _fileSystem.File.ReadAllText(_repositorySolutionAccessor.GetDirectoryBuildPropsPath()).Should().Be(expectedDirectoryBuildPropsFile);
        _fileSystem.File.ReadAllText(projectPath).Should().Be(expectedProjectFileContent);
    }
}