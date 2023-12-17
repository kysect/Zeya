using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ProjectSystemIntegration;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule(
    IFileSystem fileSystem,
    DirectoryPackagesParser directoryPackagesParser,
    RepositorySolutionAccessorFactory repositorySolutionAccessorFactory,
    ILogger logger)
    : IScenarioStepExecutor<NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.NugetVersionSynchronizedWithMasterCentralPackageManager")]
    public record Arguments(string MasterFile) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.NugetVersionSynchronizedWithMasterCentralPackageManager;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();
        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.RepositoryAccessor);

        if (!fileSystem.File.Exists(request.MasterFile))
        {
            // TODO: after this error validation should finish as failed
            logger.LogError("Master file {File} for checking CPM was not found.", request.MasterFile);
            return;
        }

        if (!repositoryValidationContext.RepositoryAccessor.Exists(repositorySolutionAccessor.GetDirectoryPackagePropsPath()))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                "Configuration file Directory.Package.props for Central Package Management is missed.",
                Arguments.DefaultSeverity);
            return;
        }

        var masterFileContent = fileSystem.File.ReadAllText(request.MasterFile);
        var masterPackages = directoryPackagesParser
            .Parse(masterFileContent)
            .ToDictionary(p => p.PackageName, p => p.Version);

        string currentProjectDirectoryPackage = fileSystem.File.ReadAllText(request.MasterFile);
        IReadOnlyCollection<NugetVersion> currentProjectPackages = directoryPackagesParser.Parse(currentProjectDirectoryPackage);

        List<string> packagesWithDifferentVersion = new List<string>();

        foreach (var nugetVersion in currentProjectPackages)
        {
            if (!masterPackages.TryGetValue(nugetVersion.PackageName, out var masterVersion))
                continue;

            if (nugetVersion.Version != masterVersion)
                packagesWithDifferentVersion.Add(nugetVersion.PackageName);
        }

        if (packagesWithDifferentVersion.Any())
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"Some Nuget packages versions is not synchronized with master Directory.Package.props: {packagesWithDifferentVersion.ToSingleString()}",
                Arguments.DefaultSeverity);
        }
    }
}