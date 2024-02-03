using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryAccess;
using Kysect.Zeya.Tests.Fakes;
using Kysect.Zeya.ValidationRules.Rules.Github;

namespace Kysect.Zeya.Tests.ValidationRules.Github;

public class GithubBranchProtectionEnabledValidationRuleTests : ValidationRuleTestBase
{
    private readonly GithubBranchProtectionEnabledValidationRule _validationRule;
    private readonly ScenarioContext _githubContext;
    private readonly FakeGithubIntegrationService _fakeGithubIntegrationService;

    public GithubBranchProtectionEnabledValidationRuleTests()
    {
        _fakeGithubIntegrationService = new FakeGithubIntegrationService();
        _validationRule = new GithubBranchProtectionEnabledValidationRule(_fakeGithubIntegrationService);
        _githubContext = RepositoryValidationContextExtensions.CreateScenarioContext(
            new RepositoryValidationContext(new ClonedGithubRepositoryAccessor(new GithubRepository("owner", "name"), CurrentPath, FileSystem),
                DiagnosticCollectorAsserts.GetCollector()));
    }

    [Fact]
    public void Execute_NoGithubRepositoryMetadata_ReturnDiagnosticAboutMissedMetadata()
    {
        var arguments = new GithubBranchProtectionEnabledValidationRule.Arguments(false, false);

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(1)
            .ShouldHaveError(1, arguments.DiagnosticCode, $"Skip {arguments.DiagnosticCode} because repository do not have GitHub metadata.");
    }

    [Fact]
    public void Execute_RepositoryWithGithubMetadata_ReturnNoDiagnostic()
    {
        var arguments = new GithubBranchProtectionEnabledValidationRule.Arguments(false, false);

        _validationRule.Execute(_githubContext, arguments);

        DiagnosticCollectorAsserts
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

        _validationRule.Execute(_githubContext, arguments);

        if (reportDiagnostic)
            DiagnosticCollectorAsserts.ShouldHaveErrorCount(0).ShouldHaveDiagnosticCount(1);
        else
            DiagnosticCollectorAsserts.ShouldHaveErrorCount(0).ShouldHaveDiagnosticCount(0);
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

        _validationRule.Execute(_githubContext, arguments);

        if (reportDiagnostic)
            DiagnosticCollectorAsserts.ShouldHaveErrorCount(0).ShouldHaveDiagnosticCount(1);
        else
            DiagnosticCollectorAsserts.ShouldHaveErrorCount(0).ShouldHaveDiagnosticCount(0);
    }
}