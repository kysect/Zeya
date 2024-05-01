using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.RepositoryValidationRules.Rules.Github;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.Github;

public class GithubAutoBranchDeletionEnabledValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly GithubAutoBranchDeletionEnabledValidationRule _validationRule;
    private readonly FakeGithubIntegrationService _fakeGithubIntegrationService;

    public GithubAutoBranchDeletionEnabledValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _fakeGithubIntegrationService = _validationTestFixture.GetRequiredService<FakeGithubIntegrationService>();
        _validationRule = new GithubAutoBranchDeletionEnabledValidationRule();
    }

    [Fact]
    public void Execute_NoGithubRepositoryMetadata_ReturnDiagnosticAboutMissedMetadata()
    {
        var arguments = new GithubAutoBranchDeletionEnabledValidationRule.Arguments();
        ScenarioContext nonGithubContext = _validationTestFixture.CreateLocalRepositoryValidationScenarioContext();

        _validationRule.Execute(nonGithubContext, arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(1)
            .ShouldHaveError(1, arguments.DiagnosticCode, $"Skip {arguments.DiagnosticCode} because repository do not have GitHub metadata.");
    }

    [Fact]
    public void Execute_RepositoryBranchProtectionDisabled_ReturnDiagnostic()
    {
        var arguments = new GithubAutoBranchDeletionEnabledValidationRule.Arguments();
        _fakeGithubIntegrationService.BranchProtectionEnabled = false;

        ScenarioContext context = _validationTestFixture.CreateGithubRepositoryValidationScenarioContext();
        _validationRule.Execute(context, arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Branch deletion on merge must be enabled.");
    }

    [Fact]
    public void Execute_RepositoryBranchProtectionEnabled_ReturnNoDiagnostic()
    {
        var arguments = new GithubAutoBranchDeletionEnabledValidationRule.Arguments();
        _fakeGithubIntegrationService.BranchProtectionEnabled = true;

        ScenarioContext context = _validationTestFixture.CreateGithubRepositoryValidationScenarioContext();
        _validationRule.Execute(context, arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }
}