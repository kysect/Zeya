using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.RepositoryValidationRules.Rules.Github;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.Github;

public class GithubBranchProtectionEnabledValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly GithubBranchProtectionEnabledValidationRule _validationRule;
    private readonly FakeGithubIntegrationService _fakeGithubIntegrationService;

    public GithubBranchProtectionEnabledValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _fakeGithubIntegrationService = _validationTestFixture.GetRequiredService<FakeGithubIntegrationService>();
        _validationRule = new GithubBranchProtectionEnabledValidationRule();
    }

    [Fact]
    public void Execute_NotGithubRepository_ReturnErrorAboutIncorrectRepository()
    {
        var arguments = new GithubBranchProtectionEnabledValidationRule.Arguments(false, false);
        var notGithubContext = _validationTestFixture.CreateLocalRepositoryValidationScenarioContext();

        _validationRule.Execute(notGithubContext, arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(1)
            .ShouldHaveError(1, arguments.DiagnosticCode, "Cannot apply github validation rule on non github repository");
    }

    [Fact]
    public void Execute_RepositoryWithGithubMetadata_ReturnNoDiagnostic()
    {
        var arguments = new GithubBranchProtectionEnabledValidationRule.Arguments(false, false);

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(0);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    public void Execute_PullRequestReviewsRequired(bool enabled, bool required, bool reportDiagnostic)
    {
        var arguments = new GithubBranchProtectionEnabledValidationRule.Arguments(required, false);
        _fakeGithubIntegrationService.RepositoryBranchProtection = new RepositoryBranchProtection(enabled, false);

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        if (reportDiagnostic)
            _validationTestFixture.DiagnosticCollectorAsserts.ShouldHaveErrorCount(0).ShouldHaveDiagnosticCount(1);
        else
            _validationTestFixture.DiagnosticCollectorAsserts.ShouldHaveErrorCount(0).ShouldHaveDiagnosticCount(0);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    public void Execute_ConversationResolutionRequired(bool enabled, bool required, bool reportDiagnostic)
    {
        var arguments = new GithubBranchProtectionEnabledValidationRule.Arguments(false, required);
        _fakeGithubIntegrationService.RepositoryBranchProtection = new RepositoryBranchProtection(false, enabled);

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        if (reportDiagnostic)
            _validationTestFixture.DiagnosticCollectorAsserts.ShouldHaveErrorCount(0).ShouldHaveDiagnosticCount(1);
        else
            _validationTestFixture.DiagnosticCollectorAsserts.ShouldHaveErrorCount(0).ShouldHaveDiagnosticCount(0);
    }
}