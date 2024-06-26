﻿using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.SourceCode;

public class ArtifactsOutputEnabledValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly ArtifactsOutputEnabledValidationRule _validationRule;

    public ArtifactsOutputEnabledValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _validationRule = new ArtifactsOutputEnabledValidationRule();
    }

    [Fact]
    public void Validate_EmptySolution_ReturnDiagnosticAboutMissedDirectoryBuildProps()
    {
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ValidationRuleMessages.DirectoryBuildPropsFileMissed);
    }

    [Fact]
    public void Validate_EmptyDirectoryBuildProps_ReturnDiagnosticAboutMissedUseArtifactsOutput()
    {
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddDirectoryBuildProps(new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty()))
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ArtifactsOutputEnabledValidationRule.Arguments.UseArtifactsOutputOptionMustBeTrue);
    }

    [Fact]
    public void Validate_UseArtifactsOutputSetToFalse_ReturnDiagnosticAboutMissedUseArtifactsOutput()
    {
        var directoryBuildPropsContent = """
                                         <Project>
                                             <PropertyGroup>
                                                 <UseArtifactsOutput>false</UseArtifactsOutput>
                                             </PropertyGroup>
                                         </Project>
                                         """;
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsContent)
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, ArtifactsOutputEnabledValidationRule.Arguments.UseArtifactsOutputOptionMustBeTrue);
    }

    [Fact]
    public void Validate_UseArtifactsOutputSetToTrue_NoDiagnostic()
    {
        var directoryBuildPropsContent = """
                                         <Project>
                                             <PropertyGroup>
                                                 <UseArtifactsOutput>true</UseArtifactsOutput>
                                             </PropertyGroup>
                                         </Project>
                                         """;
        var arguments = new ArtifactsOutputEnabledValidationRule.Arguments();

        _validationTestFixture.SolutionFileStructureBuilderFactory
            .Create("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsContent)
            .Save(_validationTestFixture.CurrentPath);

        _validationRule.Execute(_validationTestFixture.CreateLocalRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }
}