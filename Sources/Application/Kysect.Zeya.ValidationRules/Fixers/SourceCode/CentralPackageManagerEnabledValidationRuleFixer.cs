using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.Abstractions.Contracts;
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
        IReadOnlyCollection<ProjectPackageVersion> nugetPackages = CollectNugetIncludes(solutionModifier);
        nugetPackages = SelectDistinctPackages(nugetPackages);

        logger.LogDebug("Collect {Count} added Nuget packages to projects", nugetPackages.Count);

        logger.LogTrace("Apply changes to *.csproj files");
        foreach (DotnetProjectModifier solutionModifierProject in solutionModifier.Projects)
        {
            solutionModifierProject.File.UpdateDocument(ProjectNugetVersionRemover.Instance);
        }

        string directoryPackagePropsPath = repositorySolutionAccessor.GetDirectoryPackagePropsPath();
        logger.LogTrace("Apply changes to {DirectoryPackageFile} file", directoryPackagePropsPath);
        DirectoryPackagesPropsFile directoryPackagesPropsFile = solutionModifier.GetOrCreateDirectoryPackagePropsModifier();

        logger.LogDebug("Set ManagePackageVersionsCentrally to true");
        // TODO: replace with SetCentralPackageManagement 
        directoryPackagesPropsFile.File.AddOrUpdateProperty(DotnetProjectFileConstant.ManagePackageVersionsCentrally, true.ToString().ToLower());

        logger.LogDebug("Adding package versions to {DirectoryPackageFile}", directoryPackagePropsPath);
        directoryPackagesPropsFile.File.GetOrAddItemGroup();
        directoryPackagesPropsFile.File.UpdateDocument(new DirectoryPackagePropsNugetVersionAppender(nugetPackages));

        logger.LogTrace("Saving solution files");
        solutionModifier.Save(formatter);
    }

    private IReadOnlyCollection<ProjectPackageVersion> CollectNugetIncludes(DotnetSolutionModifier modifier)
    {
        var nugetVersions = new List<ProjectPackageVersion>();

        foreach (var dotnetProjectModifier in modifier.Projects)
        {
            foreach (var packageReferences in dotnetProjectModifier.File.GetPackageReferences())
            {
                if (packageReferences.Version is null)
                    continue;

                nugetVersions.Add(new ProjectPackageVersion(packageReferences.Name, packageReferences.Version));
            }
        }

        return nugetVersions;
    }

    private IReadOnlyCollection<ProjectPackageVersion> SelectDistinctPackages(IReadOnlyCollection<ProjectPackageVersion> packages)
    {
        List<ProjectPackageVersion> distinctPackages = new List<ProjectPackageVersion>();

        foreach (IGrouping<string, ProjectPackageVersion> nugetVersions in packages.GroupBy(p => p.Name))
        {
            if (nugetVersions.Count() == 1)
            {
                distinctPackages.Add(nugetVersions.Single());
                continue;
            }

            List<string> versions = nugetVersions.Select(n => n.Version).Distinct().ToList();
            if (versions.Count == 1)
            {
                distinctPackages.Add(new ProjectPackageVersion(nugetVersions.Key, versions.Single()));
                continue;
            }

            logger.LogWarning("Nuget {Package} added to projects with different versions: {Versions}", nugetVersions.Key, versions.ToSingleString());
            string selectedVersion = versions.First();
            distinctPackages.Add(new ProjectPackageVersion(nugetVersions.Key, selectedVersion));
        }

        return distinctPackages
            .OrderBy(p => p.Name)
            .ToList();
    }
}