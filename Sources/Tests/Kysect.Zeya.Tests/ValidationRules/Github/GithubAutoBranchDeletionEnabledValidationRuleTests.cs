using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Fakes;
using Kysect.Zeya.ValidationRules.Rules.Github;

namespace Kysect.Zeya.Tests.ValidationRules.Github;

public class GithubAutoBranchDeletionEnabledValidationRuleTests : ValidationRuleTestBase
{
    private readonly GithubAutoBranchDeletionEnabledValidationRule _validationRule;
    private readonly ScenarioContext _githubContext;
    private readonly FakeGithubIntegrationService _fakeGithubIntegrationService;

    public GithubAutoBranchDeletionEnabledValidationRuleTests()
    {
        _fakeGithubIntegrationService = new FakeGithubIntegrationService();
        _validationRule = new GithubAutoBranchDeletionEnabledValidationRule(_fakeGithubIntegrationService);
        _githubContext = RepositoryValidationContextExtensions.CreateScenarioContext(
            new RepositoryValidationContext(
                new GithubRepository("owner", "name"),
                Repository,
                DiagnosticCollectorAsserts.GetCollector()));
    }

    [Fact]
    public void Execute_NoGithubRepositoryMetadata_ReturnDiagnosticAboutMissedMetadata()
    {
        var arguments = new GithubAutoBranchDeletionEnabledValidationRule.Arguments();

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(1)
            .ShouldHaveError(1, arguments.DiagnosticCode, $"Skip {arguments.DiagnosticCode} because repository do not have GitHub metadata.");
    }

    [Fact]
    public void Execute_RepositoryBranchProtectionDisabled_ReturnDiagnostic()
    {
        var arguments = new GithubAutoBranchDeletionEnabledValidationRule.Arguments();
        _fakeGithubIntegrationService.BranchProtectionEnabled = false;

        _validationRule.Execute(_githubContext, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Branch deletion on merge must be enabled.");
    }

    [Fact]
    public void Execute_RepositoryBranchProtectionEnabled_ReturnNoDiagnostic()
    {
        var arguments = new GithubAutoBranchDeletionEnabledValidationRule.Arguments();
        _fakeGithubIntegrationService.BranchProtectionEnabled = true;

        _validationRule.Execute(_githubContext, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }
}