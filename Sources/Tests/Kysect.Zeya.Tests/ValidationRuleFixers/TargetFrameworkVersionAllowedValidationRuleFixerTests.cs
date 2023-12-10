﻿using Kysect.Zeya.ValidationRules.Fixers.SourceCode;
using System.IO.Abstractions.TestingHelpers;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Kysect.DotnetSlnGenerator;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.Tests.Tools;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.Zeya.ProjectSystemIntegration;

namespace Kysect.Zeya.Tests.ValidationRuleFixers;

public class TargetFrameworkVersionAllowedValidationRuleFixerTests
{
    private FakeDotnetProjectPropertyAccessor _projectPropertyAccessor;
    private TargetFrameworkVersionAllowedValidationRuleFixer _fixer;
    private MockFileSystem _fileSystem;
    private ILogger _logger;

    [SetUp]
    public void Setup()
    {
        var dotnetSolutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, _logger);

        _logger = DefaultLoggerConfiguration.CreateConsoleLogger();
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        _projectPropertyAccessor = new FakeDotnetProjectPropertyAccessor();
        _fixer = new TargetFrameworkVersionAllowedValidationRuleFixer(dotnetSolutionModifierFactory, _projectPropertyAccessor, _logger);
    }

    [Test]
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
        var solutionBuilder = new DotnetSolutionBuilder("Solution")
            .AddProject(new DotnetProjectBuilder(projectName, originalProjectContent));
        solutionBuilder.Save(_fileSystem, currentPath);

        var repositoryAccessor = new GithubRepositoryAccessor(new GithubRepository("owner", "name"), new FakePathFormatStrategy(currentPath), _fileSystem);
        _fixer.Fix(new TargetFrameworkVersionAllowedValidationRule.Arguments(["net8.0"]), repositoryAccessor);

        _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(currentPath, projectName, $"{projectName}.csproj")).Should().Be(expectedProjectContent);
    }
}