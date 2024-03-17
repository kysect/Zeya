using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;

public class NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixer(
    IFileSystem fileSystem,
    ILogger<NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixer> logger)
    : IValidationRuleFixer<NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments>
{
    public void Fix(NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments rule, ILocalRepository localRepository)
    {
        rule.ThrowIfNull();
        localRepository.ThrowIfNull();

        LocalRepositorySolution repositorySolutionAccessor = localRepository.SolutionManager.GetSolution();
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();

        DirectoryPackagesPropsFile directoryPackagesPropsFile = solutionModifier.GetOrCreateDirectoryPackagePropsModifier();
        if (!fileSystem.File.Exists(rule.MasterFile))
        {
            // TODO: after this error validation should finish as failed
            logger.LogError("Master file {File} for checking CPM was not found.", rule.MasterFile);
            return;
        }

        string masterFileContent = fileSystem.File.ReadAllText(rule.MasterFile);
        var masterPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.Create(masterFileContent));
        logger.LogDebug("Setting package versions same as in {MasterFile}", rule.MasterFile);
        foreach (ProjectPackageVersion projectPackageVersion in masterPropsFile.Versions.GetPackageVersions())
        {
            directoryPackagesPropsFile
                .Versions
                .SetPackageVersion(projectPackageVersion.Name, projectPackageVersion.Version);
        }

        solutionModifier.Save();
    }
}