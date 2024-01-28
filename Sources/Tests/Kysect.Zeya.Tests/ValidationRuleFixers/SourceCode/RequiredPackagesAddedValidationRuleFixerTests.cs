using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Fixers.SourceCode;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRuleFixers.SourceCode;

public class RequiredPackagesAddedValidationRuleFixerTests
{
    private readonly RequiredPackagesAddedValidationRuleFixer _requiredPackagesAddedValidationRuleFixer;
    private readonly string _currentPath;
    private readonly MockFileSystem _fileSystem;
    private readonly ClonedRepository _clonedRepository;
    private readonly RepositorySolutionAccessor _repositorySolutionAccessor;
    private readonly XmlDocumentSyntaxFormatter _xmlDocumentSyntaxFormatter;

    public RequiredPackagesAddedValidationRuleFixerTests()
    {
        ILogger logger = DefaultLoggerConfiguration.CreateConsoleLogger();
        _fileSystem = new MockFileSystem();
        _currentPath = _fileSystem.Path.GetFullPath(".");

        var repositorySolutionAccessorFactory = new RepositorySolutionAccessorFactory(new SolutionFileContentParser(), _fileSystem);
        _xmlDocumentSyntaxFormatter = new XmlDocumentSyntaxFormatter();
        _requiredPackagesAddedValidationRuleFixer
            = new RequiredPackagesAddedValidationRuleFixer(repositorySolutionAccessorFactory,
                _xmlDocumentSyntaxFormatter,
                logger);

        _clonedRepository = new ClonedRepository(_currentPath, _fileSystem);
        _repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(_clonedRepository);
    }

    [Fact]
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
        new SolutionFileStructureBuilder("Solution")
            .Save(_fileSystem, _currentPath, _xmlDocumentSyntaxFormatter);

        _requiredPackagesAddedValidationRuleFixer.Fix(
            arguments,
            _clonedRepository);

        _fileSystem.File.ReadAllText(_repositorySolutionAccessor.GetDirectoryBuildPropsPath()).Should().Be(expectedDirectoryBuildPropsFile);
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

        var projectPath = _fileSystem.Path.Combine(_currentPath, projectName, $"{projectName}.csproj");
        var arguments = new RequiredPackagesAddedValidationRule.Arguments(["RequiredPackage"]);
        new SolutionFileStructureBuilder("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName, projectFileContent))
            .Save(_fileSystem, _currentPath, _xmlDocumentSyntaxFormatter);

        _requiredPackagesAddedValidationRuleFixer.Fix(
            arguments,
            _clonedRepository);

        _fileSystem.File.ReadAllText(_repositorySolutionAccessor.GetDirectoryBuildPropsPath()).Should().Be(expectedDirectoryBuildPropsFile);
        _fileSystem.File.ReadAllText(projectPath).Should().Be(expectedProjectFileContent);
    }
}