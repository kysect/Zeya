﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class CentralPackageManagerEnabledValidationRuleFixer(DotnetSolutionModifierFactory dotnetSolutionModifierFactory, RepositorySolutionAccessorFactory repositorySolutionAccessorFactory, XmlDocumentSyntaxFormatter formatter, ILogger logger)
    : IValidationRuleFixer<CentralPackageManagerEnabledValidationRule.Arguments>
{
    public void Fix(CentralPackageManagerEnabledValidationRule.Arguments rule, IClonedRepository clonedRepository)
    {
        rule.ThrowIfNull();
        clonedRepository.ThrowIfNull();

        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(clonedRepository);
        string solutionPath = repositorySolutionAccessor.GetSolutionFilePath();
        DotnetSolutionModifier solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);
        IReadOnlyCollection<NugetVersion> nugetPackages = CollectNugetIncludes(solutionModifier);
        nugetPackages = SelectDistinctPackages(nugetPackages);

        logger.LogDebug("Collect {Count} added Nuget packages to projects", nugetPackages.Count);

        logger.LogTrace("Apply changes to *.csproj files");
        foreach (DotnetProjectModifier solutionModifierProject in solutionModifier.Projects)
        {
            solutionModifierProject.File.UpdateDocument(ProjectNugetVersionRemover.Instance);
        }

        string directoryPackagePropsPath = repositorySolutionAccessor.GetDirectoryPackagePropsPath();
        logger.LogTrace("Apply changes to {DirectoryPackageFile} file", directoryPackagePropsPath);
        var projectPropertyModifier = new ProjectPropertyModifier(solutionModifier.GetOrCreateDirectoryPackagePropsModifier().File, logger);

        logger.LogDebug("Set ManagePackageVersionsCentrally to true");
        projectPropertyModifier.AddOrUpdateProperty("ManagePackageVersionsCentrally", "true");

        logger.LogDebug("Adding package versions to {DirectoryPackageFile}", directoryPackagePropsPath);
        solutionModifier.GetOrCreateDirectoryPackagePropsModifier().File.UpdateDocument(AddProjectGroupNodeIfNotExistsModificationStrategy.ItemGroup);
        solutionModifier.GetOrCreateDirectoryPackagePropsModifier().File.UpdateDocument(new DirectoryPackagePropsNugetVersionAppender(nugetPackages));

        logger.LogTrace("Saving solution files");
        solutionModifier.Save(formatter);
    }

    private IReadOnlyCollection<NugetVersion> CollectNugetIncludes(DotnetSolutionModifier modifier)
    {
        var nugetVersions = new List<NugetVersion>();

        foreach (var dotnetProjectModifier in modifier.Projects)
        {
            foreach (var packageReferences in dotnetProjectModifier.File.GetPackageReferences())
            {
                if (packageReferences.Version is null)
                    continue;

                nugetVersions.Add(new NugetVersion(packageReferences.Name, packageReferences.Version));
            }
        }

        return nugetVersions;
    }

    private IReadOnlyCollection<NugetVersion> SelectDistinctPackages(IReadOnlyCollection<NugetVersion> packages)
    {
        List<NugetVersion> distinctPackages = new List<NugetVersion>();

        foreach (IGrouping<string, NugetVersion> nugetVersions in packages.GroupBy(p => p.PackageName))
        {
            if (nugetVersions.Count() == 1)
            {
                distinctPackages.Add(nugetVersions.Single());
                continue;
            }

            List<string> versions = nugetVersions.Select(n => n.Version).Distinct().ToList();
            if (versions.Count == 1)
            {
                distinctPackages.Add(new NugetVersion(nugetVersions.Key, versions.Single()));
                continue;
            }

            logger.LogWarning("Nuget {Package} added to projects with different versions: {Versions}", nugetVersions.Key, versions.ToSingleString());
            string selectedVersion = versions.First();
            distinctPackages.Add(new NugetVersion(nugetVersions.Key, selectedVersion));
        }

        return distinctPackages
            .OrderBy(p => p.PackageName)
            .ToList();
    }
}