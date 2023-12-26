using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class CentralPackageManagerEnabledValidationRuleFixer(DotnetSolutionModifierFactory dotnetSolutionModifierFactory, RepositorySolutionAccessorFactory repositorySolutionAccessorFactory, ILogger logger)
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
        foreach (var solutionModifierProject in solutionModifier.Projects)
        {
            logger.LogTrace("Remove nuget versions from {Project}", solutionModifierProject.Path);
            solutionModifierProject.Accessor.UpdateDocument(ProjectNugetVersionRemover.Instance);
        }

        string directoryPackagePropsPath = repositorySolutionAccessor.GetDirectoryPackagePropsPath();
        logger.LogTrace("Apply changes to {DirectoryPackageFile} file", directoryPackagePropsPath);
        var projectPropertyModifier = new ProjectPropertyModifier(solutionModifier.DirectoryPackagePropsModifier.Accessor, logger);

        logger.LogDebug("Set ManagePackageVersionsCentrally to true");
        projectPropertyModifier.AddOrUpdateProperty("ManagePackageVersionsCentrally", "true");

        logger.LogDebug("Adding package versions to {DirectoryPackageFile}", directoryPackagePropsPath);
        solutionModifier.DirectoryPackagePropsModifier.Accessor.UpdateDocument(AddProjectGroupNodeIfNotExistsModificationStrategy.ItemGroup);
        solutionModifier.DirectoryPackagePropsModifier.Accessor.UpdateDocument(new DirectoryPackagePropsNugetVersionAppender(nugetPackages));

        logger.LogTrace("Saving solution files");
        solutionModifier.Save();
    }

    private IReadOnlyCollection<NugetVersion> CollectNugetIncludes(DotnetSolutionModifier modifier)
    {
        var nugetVersions = new List<NugetVersion>();

        foreach (var dotnetProjectModifier in modifier.Projects)
        {
            foreach (var xmlElementSyntax in dotnetProjectModifier.Accessor.GetNodesByName("PackageReference"))
            {
                var includeAttribute = xmlElementSyntax.GetAttribute("Include");
                if (includeAttribute is null)
                    continue;

                var versionAttribute = xmlElementSyntax.GetAttribute("Version");
                if (versionAttribute is null)
                    continue;

                nugetVersions.Add(new NugetVersion(includeAttribute.Value, versionAttribute.Value));
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