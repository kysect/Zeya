﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRuleFixer(
    DotnetSolutionModifierFactory dotnetSolutionModifierFactory,
    IFileSystem fileSystem,
    RepositorySolutionAccessorFactory repositorySolutionAccessorFactory,
    XmlDocumentSyntaxFormatter formatter,
    ILogger logger)
    : IValidationRuleFixer<NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments>
{
    public void Fix(NugetVersionSynchronizedWithMasterCentralPackageManagerValidationRule.Arguments rule, IClonedRepository clonedRepository)
    {
        rule.ThrowIfNull();
        clonedRepository.ThrowIfNull();

        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(clonedRepository);
        string solutionPath = repositorySolutionAccessor.GetSolutionFilePath();
        DotnetSolutionModifier solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);

        DirectoryPackagesPropsFile directoryPackagesPropsFile = solutionModifier.GetOrCreateDirectoryPackagePropsModifier();
        // TODO: move inside DirectoryPackagesPropsFile
        bool? cpmEnabled = directoryPackagesPropsFile.File.FindBooleanProperty("ManagePackageVersionsCentrally");
        if (cpmEnabled is null or false)
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

        string masterFileContent = fileSystem.File.ReadAllText(rule.MasterFile);
        var masterPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.Create(masterFileContent));
        Dictionary<string, string> masterPackages = masterPropsFile
            .GetPackageVersions()
            .ToDictionary(p => p.Name, p => p.Version);

        logger.LogDebug("Setting package versions same as in {MasterFile}", rule.MasterFile);
        directoryPackagesPropsFile.File.UpdateDocument(new SyncCentralPackageManagementVersionsModificationStrategy(masterPackages));
        solutionModifier.Save(formatter);
    }
}