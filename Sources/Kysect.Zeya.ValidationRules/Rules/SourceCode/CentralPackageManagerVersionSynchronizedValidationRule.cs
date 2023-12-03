using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;
using Kysect.Zeya.ProjectSystemIntegration;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class CentralPackageManagerVersionSynchronizedValidationRule(
    IFileSystem fileSystem,
    DirectoryPackagesParser directoryPackagesParser,
    ILogger logger)
    : IScenarioStepExecutor<CentralPackageManagerVersionSynchronizedValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.CentralPackageManagerVersionSynchronized")]
    public record Arguments(string MasterFile) : IScenarioStep
    {
        public static string DiagnosticCode => RuleDescription.SourceCode.CentralPackageManagerVersionSynchronized;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        if (!fileSystem.File.Exists(request.MasterFile))
        {
            logger.LogError("Master file {File} for checking CPM was not found.", request.MasterFile);
            return;
        }

        var masterFileContent = fileSystem.File.ReadAllText(request.MasterFile);
        var masterPackages = directoryPackagesParser
            .Parse(masterFileContent)
            .ToDictionary(p => p.PackageName, p => p.Version);

        if (!repositoryValidationContext.RepositoryAccessor.Exists(ValidationConstants.DirectoryPackagePropsPath))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                "Configuration file Directory.Package.props for Central Package Management is missed.",
                Arguments.DefaultSeverity);
            return;
        }

        string currentProjectDirectoryPackage = fileSystem.File.ReadAllText(ValidationConstants.DirectoryPackagePropsPath);
        IReadOnlyCollection<NugetVersion> currentProjectPackages = directoryPackagesParser.Parse(currentProjectDirectoryPackage);

        foreach (var nugetVersion in currentProjectPackages)
        {
            if (!masterPackages.TryGetValue(nugetVersion.PackageName, out var masterVersion))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    Arguments.DiagnosticCode,
                    $"Nuget {nugetVersion.PackageName} version is not specified in master Directory.Package.props",
                    Arguments.DefaultSeverity);
                continue;
            }

            if (nugetVersion.Version != masterVersion)
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    Arguments.DiagnosticCode,
                    $"Nuget {nugetVersion.PackageName} should be {masterVersion} but was {nugetVersion.Version}",
                    Arguments.DefaultSeverity);
            }
        }
    }
}