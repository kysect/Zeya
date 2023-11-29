using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ManagedDotnetCli;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules;

[ScenarioStep("ValidateTargetFrameworkVersion")]
public record ValidateTargetFrameworkVersion(IReadOnlyCollection<string> AllowedVersions) : IScenarioStep
{
    public static string DiagnosticCode = "SRC00003";
    public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;

    public static string GetMessage(ValidateTargetFrameworkVersion request, string currentVersions)
    {
        return $"Framework versions {currentVersions} is not allowed.";
    }
}

public class ValidateTargetFrameworkVersionValidationRule : IScenarioStepExecutor<ValidateTargetFrameworkVersion>
{
    private readonly IFileSystem _fileSystem;
    private readonly DotnetCli _dotnetCli;

    public ValidateTargetFrameworkVersionValidationRule(IFileSystem fileSystem, DotnetCli dotnetCli)
    {
        _fileSystem = fileSystem;
        _dotnetCli = dotnetCli;
    }

    public void Execute(ScenarioContext context, ValidateTargetFrameworkVersion request)
    {
        var allowedTargetFrameworks = request.AllowedVersions.ToHashSet();
        var repositoryValidationContext = context.GetValidationContext();

        IEnumerable<string> projectFiles = _fileSystem.Directory.EnumerateFiles(repositoryValidationContext.RepositoryAccessor.GetFullPath(), "*.csproj", SearchOption.AllDirectories);

        foreach (var projectFile in projectFiles)
        {
            var projectPropertyAccessor = new DotnetProjectPropertyAccessor(projectFile, _dotnetCli);
            var targetFramework = projectPropertyAccessor.GetTargetFramework();

            if (!allowedTargetFrameworks.Contains(targetFramework))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    ValidateTargetFrameworkVersion.DiagnosticCode,
                    ValidateTargetFrameworkVersion.GetMessage(request, targetFramework),
                    ValidateTargetFrameworkVersion.DefaultSeverity);
            }
        }
    }
}