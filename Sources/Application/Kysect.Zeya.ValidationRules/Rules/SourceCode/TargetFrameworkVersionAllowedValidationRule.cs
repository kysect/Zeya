using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class TargetFrameworkVersionAllowedValidationRule(IDotnetProjectPropertyAccessor projectPropertyAccessor, RepositorySolutionAccessorFactory repositorySolutionAccessorFactory)
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

        List<string> projectFiles = repositorySolutionAccessor
            .GetProjectPaths()
            .ToList();

        foreach (var projectFile in projectFiles)
        {
            var targetFramework = projectPropertyAccessor.GetTargetFramework(projectFile);

            if (!allowedTargetFrameworks.Contains(targetFramework))
            {
                repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                    request.DiagnosticCode,
                    $"Framework versions {targetFramework} is not allowed but used in {projectFile}.",
                    Arguments.DefaultSeverity);
            }
        }
    }
}