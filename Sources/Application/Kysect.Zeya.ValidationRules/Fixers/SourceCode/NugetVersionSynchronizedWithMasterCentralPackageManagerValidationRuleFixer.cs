using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixer(
    DotnetSolutionModifierFactory dotnetSolutionModifierFactory,
    IFileSystem fileSystem,
    DirectoryPackagesParser directoryPackagesParser,
    ILogger logger)
    : IValidationRuleFixer<NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments>
{
    public void Fix(NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments rule, IGithubRepositoryAccessor githubRepository)
    {
        rule.ThrowIfNull();
        githubRepository.ThrowIfNull();

        var solutionPath = githubRepository.GetSolutionFilePath();
        var solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);

        if (solutionModifier.DirectoryPackagePropsModifier.Accessor.IsEmpty())
        {
            logger.LogWarning("Project is not use CPM, skip version fixer");
            return;
        }

        if (!fileSystem.File.Exists(rule.MasterFile))
        {
            // TODO: after this error validation should finish as failed
            logger.LogError("Master file {File} for checking CPM was not found.", rule.MasterFile);
            return;
        }

        var masterFileContent = fileSystem.File.ReadAllText(rule.MasterFile);
        var masterPackages = directoryPackagesParser
            .Parse(masterFileContent)
            .ToDictionary(p => p.PackageName, p => p.Version);

        logger.LogDebug("Setting package versions same as in {MasterFile}", rule.MasterFile);
        solutionModifier.DirectoryPackagePropsModifier.Accessor.UpdateDocument(new SyncCentralPackageManagementVersionsModificationStrategy(masterPackages));
        solutionModifier.Save();
    }
}