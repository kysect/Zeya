﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration.Abstraction.Contracts;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.Models;
using Kysect.Zeya.ValidationRules.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules.Github;

public class GithubAutoBranchDeletionEnabledValidationRule(IGithubIntegrationService githubIntegrationService)
    : IScenarioStepExecutor<GithubAutoBranchDeletionEnabledValidationRule.Arguments>
{
    [ScenarioStep("Github.AutoBranchDeletionEnabled")]
    public record Arguments() : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Github.AutoBranchDeletionEnabled;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        if (repositoryValidationContext.Repository is not ClonedGithubRepositoryAccessor clonedGithubRepository)
        {
            repositoryValidationContext.DiagnosticCollector.AddRuntimeError(
                request.DiagnosticCode,
                $"Skip {request.DiagnosticCode} because repository do not have GitHub metadata.");

            return;
        }

        if (!githubIntegrationService.DeleteBranchOnMerge(clonedGithubRepository.GithubMetadata))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                "Branch deletion on merge must be enabled.",
                Arguments.DefaultSeverity);
        }
    }
}