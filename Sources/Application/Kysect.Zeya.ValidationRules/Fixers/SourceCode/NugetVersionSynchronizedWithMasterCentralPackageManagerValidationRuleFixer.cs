using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.ValidationRules.Abstractions;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixer(
    IFileSystem fileSystem,
    XmlDocumentSyntaxFormatter formatter,
    ILogger logger)
    : IValidationRuleFixer<NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments>
{
    public void Fix(NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments rule, IClonedRepository clonedRepository)
    {
        rule.ThrowIfNull();
        clonedRepository.ThrowIfNull();

        LocalRepositorySolution repositorySolutionAccessor = clonedRepository.SolutionManager.GetSolution();
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

        solutionModifier.Save(formatter);
    }
}