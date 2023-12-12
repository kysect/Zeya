using Kysect.DotnetSlnParser.Parsers;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ManagedDotnetCli;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class TargetFrameworkVersionAllowedValidationRule(IDotnetProjectPropertyAccessor projectPropertyAccessor, SolutionFileParser solutionFileParser)
    : IScenarioStepExecutor<TargetFrameworkVersionAllowedValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.TargetFrameworkVersionAllowed")]
    public record Arguments(IReadOnlyCollection<string> AllowedVersions) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.TargetFrameworkVersionAllowed;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        var allowedTargetFrameworks = request.AllowedVersions.ToHashSet();
        var repositorySolutionAccessor = new RepositorySolutionAccessor(repositoryValidationContext.RepositoryAccessor, solutionFileParser);
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