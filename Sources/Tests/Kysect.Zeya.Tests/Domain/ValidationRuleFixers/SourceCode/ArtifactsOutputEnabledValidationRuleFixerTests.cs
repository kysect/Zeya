﻿using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.ValidationRuleFixers.SourceCode;

public class ArtifactsOutputEnabledValidationRuleFixerTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly ArtifactsOutputEnabledValidationRuleFixer _fixer;

    public ArtifactsOutputEnabledValidationRuleFixerTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _fixer = _validationTestFixture.GetRequiredService<ArtifactsOutputEnabledValidationRuleFixer>();
    }

    [Fact]
    public void Fix_SolutionWithoutDirectoryBuildProps_CreateWithArtifactOutput()
    {
        var expectedDirectoryBuildProps = """
                                          <Project>
                                            <PropertyGroup>
                                              <UseArtifactsOutput>true</UseArtifactsOutput>
                                            </PropertyGroup>
                                          </Project>
                                          """;

        _validationTestFixture.SolutionFileStructureBuilderFactory.Create("Solution")
            .Save(_validationTestFixture.CurrentPath);

        ILocalRepository localGithubRepository = _validationTestFixture.CreateLocalRepository();
        _fixer.Fix(new ArtifactsOutputEnabledValidationRule.Arguments(), localGithubRepository);

        _validationTestFixture.FileSystemAsserts
            .File(_validationTestFixture.CurrentPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expectedDirectoryBuildProps);
    }
}