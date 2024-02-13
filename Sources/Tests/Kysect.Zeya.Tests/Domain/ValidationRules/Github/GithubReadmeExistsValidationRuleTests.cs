using Kysect.Zeya.RepositoryValidationRules.Rules.Github;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.Github;

public class GithubReadmeExistsValidationRuleTests : ValidationRuleTestBase
{
    private readonly GithubReadmeExistsValidationRule _validationRule;

    public GithubReadmeExistsValidationRuleTests()
    {
        _validationRule = new GithubReadmeExistsValidationRule();
    }

    [Fact]
    public void Execute_EmptyRepository_ReturnDiagnosticAboutMissedReadme()
    {
        var arguments = new GithubReadmeExistsValidationRule.Arguments();

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Readme.md file was not found");
    }

    [Fact]
    public void Execute_RepositoryWithoutReadme_ReturnNoDiagnostic()
    {
        var arguments = new GithubReadmeExistsValidationRule.Arguments();
        FileSystem.AddFile("Readme.md", new MockFileData(string.Empty));

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }
}