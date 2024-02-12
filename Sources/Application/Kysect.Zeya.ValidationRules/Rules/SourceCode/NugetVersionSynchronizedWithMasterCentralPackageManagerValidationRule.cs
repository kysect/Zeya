using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation.Abstractions;
using Kysect.Zeya.RepositoryValidation.Abstractions.Models;
using Kysect.Zeya.ValidationRules.Abstractions;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule(
    IFileSystem fileSystem)
    : IScenarioStepExecutor<NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.NugetVersionSynchronizedWithMasterCentralPackageManager")]
    public record Arguments(string MasterFile) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.NugetVersionSynchronizedWithMasterCentralPackageManager;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
        public const string DirectoryPackagePropsFileMissed = "Configuration file Directory.Package.props for Central Package Management is missed.";
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        LocalRepositorySolution repositorySolutionAccessor = repositoryValidationContext.Repository.SolutionManager.GetSolution();
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();

        if (!fileSystem.File.Exists(request.MasterFile))
        {
            repositoryValidationContext.DiagnosticCollector.AddRuntimeError(
                request.DiagnosticCode,
                $"Master file {request.MasterFile} for checking CPM was not found.");
            return;
        }

        if (!repositoryValidationContext.Repository.FileSystem.Exists(repositorySolutionAccessor.GetDirectoryPackagePropsPath()))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                Arguments.DirectoryPackagePropsFileMissed,
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

        var notSynchronizedPackages = new List<string>();

        foreach (ProjectPackageVersion nugetVersion in currentProjectPackages)
        {
            if (!masterPackages.TryGetValue(nugetVersion.Name, out string? masterVersion))
            {
                notSynchronizedPackages.Add(nugetVersion.Name);
                continue;
            }

            if (nugetVersion.Version != masterVersion)
                notSynchronizedPackages.Add(nugetVersion.Name);
        }

        if (notSynchronizedPackages.Any())
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Some Nuget packages versions is not synchronized with master Directory.Package.props: {notSynchronizedPackages.ToSingleString()}",
                Arguments.DefaultSeverity);
        }
    }
}