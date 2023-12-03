using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ProjectSystemIntegration;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class SourceCodeRequiredPackagesAddedValidationRule(IFileSystem fileSystem, ILogger logger) : IScenarioStepExecutor<SourceCodeRequiredPackagesAddedValidationRule.Required>
{
    [ScenarioStep("SourceCode.RequiredPackagesAdded")]
    public record Required(IReadOnlyCollection<string> Packages) : IScenarioStep
    {
        public static string DiagnosticCode => RuleDescription.SourceCode.RequiredPackagesAdded;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Required request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        var filePath = Path.Combine(repositoryValidationContext.RepositoryAccessor.GetFullPath(), "Sources", "Directory.Build.props");
        if (!fileSystem.File.Exists(filePath))
        {
            logger.LogError("Directory.Build.props file is missed.");
            return;
        }

        var directoryBuildProps = fileSystem.File.ReadAllText(filePath);
        var parser = new DirectoryBuildPropsParser();
        var addedPackages = parser.GetListOfPackageReference(directoryBuildProps).ToHashSet();

        foreach (var requestPackage in request.Packages)
        {
            if (!addedPackages.Contains(requestPackage))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    Required.DiagnosticCode,
                    $"Package {requestPackage} is not add to all solution.",
                    Required.DefaultSeverity);
            }
        }
    }
}