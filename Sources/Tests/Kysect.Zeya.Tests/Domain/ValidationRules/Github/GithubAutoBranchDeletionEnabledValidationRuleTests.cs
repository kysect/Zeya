﻿using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.Github;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.Github;

public class GithubAutoBranchDeletionEnabledValidationRuleTests : ValidationRuleTestBase
{
    private readonly GithubAutoBranchDeletionEnabledValidationRule _validationRule;
    private readonly FakeGithubIntegrationService _fakeGithubIntegrationService;

    public GithubAutoBranchDeletionEnabledValidationRuleTests()
    {
        _fakeGithubIntegrationService = FakeGithubIntegrationServiceTestInstance.Create();
        _validationRule = new GithubAutoBranchDeletionEnabledValidationRule(_fakeGithubIntegrationService);
    }

    [Fact]
    public void Execute_NoGithubRepositoryMetadata_ReturnDiagnosticAboutMissedMetadata()
    {
        var arguments = new GithubAutoBranchDeletionEnabledValidationRule.Arguments();
        ScenarioContext nonGithubContext = RepositoryValidationContextExtensions.CreateScenarioContext(
            new RepositoryValidationContext(new LocalRepository(CurrentPath, FileSystem), DiagnosticCollectorAsserts.GetCollector()));

        _validationRule.Execute(nonGithubContext, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveErrorCount(1)
            .ShouldHaveError(1, arguments.DiagnosticCode, $"Skip {arguments.DiagnosticCode} because repository do not have GitHub metadata.");
    }

    [Fact]
    public void Execute_RepositoryBranchProtectionDisabled_ReturnDiagnostic()
    {
        var arguments = new GithubAutoBranchDeletionEnabledValidationRule.Arguments();
        _fakeGithubIntegrationService.BranchProtectionEnabled = false;

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "Branch deletion on merge must be enabled.");
    }

    [Fact]
    public void Execute_RepositoryBranchProtectionEnabled_ReturnNoDiagnostic()
    {
        var arguments = new GithubAutoBranchDeletionEnabledValidationRule.Arguments();
        _fakeGithubIntegrationService.BranchProtectionEnabled = true;

        _validationRule.Execute(Context, arguments);

        DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(0);
    }
}