using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryAccess;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class TargetFrameworkVersionAllowedValidationRule(RepositorySolutionAccessorFactory repositorySolutionAccessorFactory)
    : IScenarioStepExecutor<TargetFrameworkVersionAllowedValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.TargetFrameworkVersionAllowed")]
    public record Arguments(string? AllowedCoreVersion, string? AllowedStandardVersion, string? AllowedFrameworkVersion) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.TargetFrameworkVersionAllowed;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        HashSet<string> allowedTargetFrameworks = new HashSet<string>();

        if (request.AllowedCoreVersion is not null)
            allowedTargetFrameworks.Add(request.AllowedCoreVersion);

        if (request.AllowedStandardVersion is not null)
            allowedTargetFrameworks.Add(request.AllowedStandardVersion);

        if (request.AllowedFrameworkVersion is not null)
            allowedTargetFrameworks.Add(request.AllowedFrameworkVersion);


        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.Repository);
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();

        List<string> projectFiles = repositorySolutionAccessor
            .GetProjectPaths()
            .ToList();

        foreach ((string key, DotnetCsprojFile value) in solutionModifier.Projects)
        {
            var targetFramework = value.File.Properties.GetProperty("TargetFramework");

            if (!allowedTargetFrameworks.Contains(targetFramework.Value))
            {
                repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                    request.DiagnosticCode,
                    $"Framework versions {targetFramework} is not allowed but used in {key}.",
                    Arguments.DefaultSeverity);
            }
        }
    }
}