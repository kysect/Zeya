﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

namespace Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;

public class TargetFrameworkVersionAllowedValidationRule()
    : IScenarioStepExecutor<TargetFrameworkVersionAllowedValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.TargetFrameworkVersionAllowed")]
    public record Arguments(
        string? AllowedCoreVersion,
        string? AllowedStandardVersion,
        string? AllowedFrameworkVersion) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.TargetFrameworkVersionAllowed;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var allowedTargetFrameworks = new HashSet<string>();

        if (request.AllowedCoreVersion is not null)
            allowedTargetFrameworks.Add(request.AllowedCoreVersion);

        if (request.AllowedStandardVersion is not null)
            allowedTargetFrameworks.Add(request.AllowedStandardVersion);

        if (request.AllowedFrameworkVersion is not null)
            allowedTargetFrameworks.Add(request.AllowedFrameworkVersion);


        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        LocalRepositorySolution repositorySolutionAccessor = repositoryValidationContext.Repository.SolutionManager.GetSolution();
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();

        foreach ((string key, DotnetCsprojFile value) in solutionModifier.Projects)
        {
            DotnetProjectProperty? targetFramework = value.File.Properties.FindProperty("TargetFramework");
            if (targetFramework is not null)
            {
                if (!allowedTargetFrameworks.Contains(targetFramework.Value.Value))
                {
                    repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                        request.DiagnosticCode,
                        $"TargetFramework {targetFramework} is not allowed but used in {key}.",
                        Arguments.DefaultSeverity);
                }

                continue;
            }

            DotnetProjectProperty? propsFileTargetFramework = solutionModifier.GetOrCreateDirectoryBuildPropsModifier().File.Properties.FindProperty("TargetFramework");
            if (propsFileTargetFramework is not null)
            {
                if (!allowedTargetFrameworks.Contains(propsFileTargetFramework.Value.Value))
                {
                    repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                        request.DiagnosticCode,
                        $"TargetFramework {targetFramework} from Directory.Build.props is not allowed but used in {key}.",
                        Arguments.DefaultSeverity);
                }

                continue;
            }

            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"TargetFramework is not specified for project {key}.",
                Arguments.DefaultSeverity);
        }
    }
}