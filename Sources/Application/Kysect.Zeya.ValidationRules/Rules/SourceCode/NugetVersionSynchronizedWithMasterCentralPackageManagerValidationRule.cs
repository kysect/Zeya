using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule(
    IFileSystem fileSystem,
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
        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.Repository);

        if (!fileSystem.File.Exists(request.MasterFile))
        {
            // TODO: after this error validation should finish as failed
            logger.LogError("Master file {File} for checking CPM was not found.", request.MasterFile);
            return;
        }

        if (!repositoryValidationContext.Repository.Exists(repositorySolutionAccessor.GetDirectoryPackagePropsPath()))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                "Configuration file Directory.Package.props for Central Package Management is missed.",
                Arguments.DefaultSeverity);
            return;
        }

        var masterFileContent = fileSystem.File.ReadAllText(request.MasterFile);
        var masterPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.Create(masterFileContent));
        var masterPackages = masterPropsFile
            .Versions
            .GetPackageVersions()
            .ToDictionary(p => p.Name, p => p.Version);

        string directoryPackagesFileContent = fileSystem.File.ReadAllText(repositorySolutionAccessor.GetDirectoryPackagePropsPath());
        var directoryPackagesFile = new DirectoryPackagesPropsFile(DotnetProjectFile.Create(directoryPackagesFileContent));
        IReadOnlyCollection<ProjectPackageVersion> currentProjectPackages = directoryPackagesFile.Versions.GetPackageVersions();

        List<string> packagesWithDifferentVersion = new List<string>();

        foreach (var nugetVersion in currentProjectPackages)
        {
            if (!masterPackages.TryGetValue(nugetVersion.Name, out var masterVersion))
                continue;

            if (nugetVersion.Version != masterVersion)
                packagesWithDifferentVersion.Add(nugetVersion.Name);
        }

        if (packagesWithDifferentVersion.Any())
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Some Nuget packages versions is not synchronized with master Directory.Package.props: {packagesWithDifferentVersion.ToSingleString()}",
                Arguments.DefaultSeverity);
        }
    }
}