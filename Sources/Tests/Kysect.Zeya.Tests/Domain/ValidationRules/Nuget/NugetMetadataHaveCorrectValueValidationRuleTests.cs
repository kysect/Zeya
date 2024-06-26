﻿using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.Nuget;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.Nuget;

public class NugetMetadataHaveCorrectValueValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly NugetMetadataHaveCorrectValueValidationRule _validationRule;

    public NugetMetadataHaveCorrectValueValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _validationRule = new NugetMetadataHaveCorrectValueValidationRule();
    }

    [Fact]
    public void Execute_NoDirectoryBuildPropsFile_ReturnDiagnosticAboutMissedDirectoryBuildProps()
    {
        var arguments = new NugetMetadataHaveCorrectValueValidationRule.Arguments([]);

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ValidationRuleMessages.DirectoryBuildPropsFileMissed);
    }

    [Fact]
    public void Execute_NoRules_NoDiagnostics()
    {
        var arguments = new NugetMetadataHaveCorrectValueValidationRule.Arguments([]);
        var directoryBuildPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty());

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsFile)
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(0);
    }

    [Fact]
    public void Execute_RequiredPropertyMissed_ReturnDiagnosticAboutMissedProperty()
    {
        var arguments = new NugetMetadataHaveCorrectValueValidationRule.Arguments(new Dictionary<string, string> { { "RequiredProperty", "true" } });
        var directoryBuildPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty());

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsFile)
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Directory.Build.props options has incorrect value or value is missed: RequiredProperty");
    }

    [Fact]
    public void Execute_RequiredPropertySpecifiedWithCorrectValue_ReturnNoDiagnostic()
    {
        var arguments = new NugetMetadataHaveCorrectValueValidationRule.Arguments(new Dictionary<string, string> { { "RequiredProperty", "true" } });
        var directoryBuildPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty());
        directoryBuildPropsFile.File.Properties.SetProperty("RequiredProperty", "true");

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsFile)
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(0);
    }

    [Fact]
    public void Execute_RequiredPropertySpecifiedWithDifferentValue_ReturnDiagnostic()
    {
        var arguments = new NugetMetadataHaveCorrectValueValidationRule.Arguments(new Dictionary<string, string> { { "RequiredProperty", "true" } });
        var directoryBuildPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty());
        directoryBuildPropsFile.File.Properties.SetProperty("RequiredProperty", "false");

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsFile)
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0)
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Directory.Build.props options has incorrect value or value is missed: RequiredProperty");
    }
}