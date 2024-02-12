﻿using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.Github;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;

namespace Kysect.Zeya.Tests.ValidationRules.Github;

public class GithubBranchProtectionEnabledValidationRuleTests : ValidationRuleTestBase
{
    private readonly GithubBranchProtectionEnabledValidationRule _validationRule;
    private readonly FakeGithubIntegrationService _fakeGithubIntegrationService;

    public GithubBranchProtectionEnabledValidationRuleTests()
    {
        _fakeGithubIntegrationService = FakeGithubIntegrationServiceTestInstance.Create();
        _validationRule = new GithubBranchProtectionEnabledValidationRule(_fakeGithubIntegrationService);
    }

    [Fact]
    public void Execute_NoGithubRepositoryMetadata_ReturnDiagnosticAboutMissedMetadata()
    {
        var arguments = new GithubBranchProtectionEnabledValidationRule.Arguments(false, false);
        ScenarioContext nonGithubContext = RepositoryValidationContextExtensions.CreateScenarioContext(
            new RepositoryValidationContext(new LocalRepository(CurrentPath, FileSystem), DiagnosticCollectorAsserts.GetCollector()));

        _validationRule.Execute(nonGithubContext, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(1)
            .ShouldHaveError(1, arguments.DiagnosticCode, $"Skip {arguments.DiagnosticCode} because repository do not have GitHub metadata.");
    }

    [Fact]
    public void Execute_RepositoryWithGithubMetadata_ReturnNoDiagnostic()
    {
        var arguments = new GithubBranchProtectionEnabledValidationRule.Arguments(false, false);

        _validationRule.Execute(Context, arguments);

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

        _validationRule.Execute(Context, arguments);

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

        _validationRule.Execute(Context, arguments);

        if (reportDiagnostic)
            DiagnosticCollectorAsserts.ShouldHaveErrorCount(0).ShouldHaveDiagnosticCount(1);
        else
            DiagnosticCollectorAsserts.ShouldHaveErrorCount(0).ShouldHaveDiagnosticCount(0);
    }
}