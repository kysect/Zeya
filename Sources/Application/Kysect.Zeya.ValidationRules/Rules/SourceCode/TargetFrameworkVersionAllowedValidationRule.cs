using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ManagedDotnetCli;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class TargetFrameworkVersionAllowedValidationRule(IFileSystem fileSystem, DotnetCli dotnetCli) : IScenarioStepExecutor<TargetFrameworkVersionAllowedValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.TargetFrameworkVersionAllowed")]
    public record Arguments(IReadOnlyCollection<string> AllowedVersions) : IScenarioStep
    {
        public static string DiagnosticCode => RuleDescription.SourceCode.TargetFrameworkVersionAllowed;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        var allowedTargetFrameworks = request.AllowedVersions.ToHashSet();

        var projectFiles = fileSystem
            .Directory
            .EnumerateFiles(repositoryValidationContext.RepositoryAccessor.GetFullPath(), "*.csproj", SearchOption.AllDirectories)
            .ToList();

        foreach (var projectFile in projectFiles)
        {
            var projectPropertyAccessor = new DotnetProjectPropertyAccessor(projectFile, dotnetCli);
            var targetFramework = projectPropertyAccessor.GetTargetFramework();

            if (!allowedTargetFrameworks.Contains(targetFramework))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    Arguments.DiagnosticCode,
                    $"Framework versions {targetFramework} is not allowed but used in {projectFile}.",
                    Arguments.DefaultSeverity);
            }
        }
    }
}