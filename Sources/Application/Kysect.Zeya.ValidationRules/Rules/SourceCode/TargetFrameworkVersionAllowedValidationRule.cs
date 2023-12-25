using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class TargetFrameworkVersionAllowedValidationRule(IDotnetProjectPropertyAccessor projectPropertyAccessor, RepositorySolutionAccessorFactory repositorySolutionAccessorFactory)
    : IScenarioStepExecutor<TargetFrameworkVersionAllowedValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.TargetFrameworkVersionAllowed")]
    public record Arguments(IReadOnlyCollection<string> AllowedVersions) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.TargetFrameworkVersionAllowed;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();

        var allowedTargetFrameworks = request.AllowedVersions.ToHashSet();
        var repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.Repository);
        var projectFiles = repositorySolutionAccessor
            .GetProjectPaths()
            .ToList();

        foreach (var projectFile in projectFiles)
        {
            var targetFramework = projectPropertyAccessor.GetTargetFramework(projectFile);

            if (!allowedTargetFrameworks.Contains(targetFramework))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    request.DiagnosticCode,
                    $"Framework versions {targetFramework} is not allowed but used in {projectFile}.",
                    Arguments.DefaultSeverity);
            }
        }
    }
}