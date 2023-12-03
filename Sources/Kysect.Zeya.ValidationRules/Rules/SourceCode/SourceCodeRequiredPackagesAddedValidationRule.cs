using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ProjectSystemIntegration;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class SourceCodeRequiredPackagesAddedValidationRule(IFileSystem fileSystem, ILogger logger) : IScenarioStepExecutor<SourceCodeRequiredPackagesAddedValidationRule.Argument>
{
    [ScenarioStep("SourceCode.RequiredPackagesAdded")]
    public record Argument(IReadOnlyCollection<string> Packages) : IScenarioStep
    {
        public static string DiagnosticCode => RuleDescription.SourceCode.RequiredPackagesAdded;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Argument request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        if (!repositoryValidationContext.RepositoryAccessor.Exists(ValidationConstants.DirectoryBuildPropsPath))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                DirectoryBuildPropsContainsRequiredFields.DiagnosticCode,
                "Directory.Build.props file is not exists.",
                DirectoryBuildPropsContainsRequiredFields.DefaultSeverity);
            return;
        }

        var directoryBuildProps = repositoryValidationContext.RepositoryAccessor.ReadFile(ValidationConstants.DirectoryBuildPropsPath);
        var parser = new DirectoryBuildPropsParser();
        var addedPackages = parser.GetListOfPackageReference(directoryBuildProps).ToHashSet();

        foreach (var requestPackage in request.Packages)
        {
            if (!addedPackages.Contains(requestPackage))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    Argument.DiagnosticCode,
                    $"Package {requestPackage} is not add to all solution.",
                    Argument.DefaultSeverity);
            }
        }
    }
}