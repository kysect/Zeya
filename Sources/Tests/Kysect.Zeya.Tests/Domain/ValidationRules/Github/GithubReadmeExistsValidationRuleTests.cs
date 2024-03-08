using Kysect.Zeya.RepositoryValidationRules.Rules.Github;
using Kysect.Zeya.Tests.Tools;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.Github;

public class GithubReadmeExistsValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly GithubReadmeExistsValidationRule _validationRule;

    public GithubReadmeExistsValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _validationRule = new GithubReadmeExistsValidationRule();
    }

    [Fact]
    public void Execute_EmptyRepository_ReturnDiagnosticAboutMissedReadme()
    {
        var arguments = new GithubReadmeExistsValidationRule.Arguments();

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Readme.md file was not found");
    }

    [Fact]
    public void Execute_RepositoryWithoutReadme_ReturnNoDiagnostic()
    {
        var arguments = new GithubReadmeExistsValidationRule.Arguments();
        _validationTestFixture.FileSystem.AddFile("Readme.md", new MockFileData(string.Empty));

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }
}