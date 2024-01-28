using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Fixers.SourceCode;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRuleFixers;

public class TargetFrameworkVersionAllowedValidationRuleFixerTests
{
    private readonly FakeDotnetProjectPropertyAccessor _projectPropertyAccessor;
    private readonly TargetFrameworkVersionAllowedValidationRuleFixer _fixer;
    private readonly MockFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly XmlDocumentSyntaxFormatter _xmlDocumentSyntaxFormatter;

    public TargetFrameworkVersionAllowedValidationRuleFixerTests()
    {
        _xmlDocumentSyntaxFormatter = new XmlDocumentSyntaxFormatter();
        _logger = DefaultLoggerConfiguration.CreateConsoleLogger();
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        _projectPropertyAccessor = new FakeDotnetProjectPropertyAccessor();
        var dotnetSolutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, new SolutionFileContentParser());
        _fixer = new TargetFrameworkVersionAllowedValidationRuleFixer(
            dotnetSolutionModifierFactory,
            _projectPropertyAccessor,
            new RepositorySolutionAccessorFactory(new SolutionFileContentParser(), _fileSystem),
            _xmlDocumentSyntaxFormatter,
            _logger);
    }

    [Fact]
    public void Fix_Net6_ChangedToNet8()
    {
        var originalProjectContent = """
                                     <Project Sdk="Microsoft.NET.Sdk">
                                       <PropertyGroup>
                                         <TargetFramework>net6.0</TargetFramework>
                                       </PropertyGroup>
                                     </Project>
                                     """;

        var expectedProjectContent = """
                                     <Project Sdk="Microsoft.NET.Sdk">
                                       <PropertyGroup>
                                         <TargetFramework>net8.0</TargetFramework>
                                       </PropertyGroup>
                                     </Project>
                                     """;

        // TODO: this is hack because target framework version is fetched from dotnet CLI. Need to rework in future
        _projectPropertyAccessor.TargetFramework = "net6.0";

        string currentPath = _fileSystem.Path.GetFullPath(".");
        const string projectName = "SampleProject";
        var solutionBuilder = new SolutionFileStructureBuilder("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName, originalProjectContent));
        solutionBuilder.Save(_fileSystem, currentPath, _xmlDocumentSyntaxFormatter);

        var githubRepositoryAccessorFactory = new GithubRepositoryAccessorFactory(new FakePathFormatStrategy(currentPath), _fileSystem);
        var repositoryAccessor = githubRepositoryAccessorFactory.Create(new GithubRepository("owner", "name"));
        _fixer.Fix(new TargetFrameworkVersionAllowedValidationRule.Arguments(["net8.0"]), repositoryAccessor);

        _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(currentPath, projectName, $"{projectName}.csproj")).Should().Be(expectedProjectContent);
    }
}