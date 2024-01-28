using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class NugetVersionMustBeSpecifiedInMasterCentralPackageManagerValidationRule(
    IFileSystem fileSystem,
    RepositorySolutionAccessorFactory repositorySolutionAccessorFactory,
    ILogger logger)
    : IScenarioStepExecutor<NugetVersionMustBeSpecifiedInMasterCentralPackageManagerValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.NugetVersionMustBeSpecifiedInMasterCentralPackageManager")]
    public record Arguments(string MasterFile) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.NugetVersionMustBeSpecifiedInMasterCentralPackageManager;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.Repository);
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();

        if (!fileSystem.File.Exists(request.MasterFile))
        {
            // TODO: after this error validation should finish as failed
            logger.LogError("Master file {File} for checking CPM was not found.", request.MasterFile);
            return;
        }

        if (!repositoryValidationContext.Repository.Exists(repositorySolutionAccessor.GetDirectoryPackagePropsPath()))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
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

        DirectoryPackagesPropsFile currentPackagesPropsFile = solutionModifier.GetOrCreateDirectoryPackagePropsModifier();
        IReadOnlyCollection<ProjectPackageVersion> currentProjectPackages = currentPackagesPropsFile.Versions.GetPackageVersions();

        var notSpecifiedPackages = new List<string>();

        foreach (ProjectPackageVersion nugetVersion in currentProjectPackages)
        {
            if (!masterPackages.TryGetValue(nugetVersion.Name, out _))
                notSpecifiedPackages.Add(nugetVersion.Name);
        }

        if (notSpecifiedPackages.Any())
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"Some Nuget packages is not specified in master Directory.Package.props: {notSpecifiedPackages.ToSingleString()}",
                Arguments.DefaultSeverity);
        }
    }
}