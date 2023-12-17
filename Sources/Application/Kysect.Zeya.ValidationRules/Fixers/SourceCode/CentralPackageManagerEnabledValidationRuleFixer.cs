using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class CentralPackageManagerEnabledValidationRuleFixer(DotnetSolutionModifierFactory dotnetSolutionModifierFactory, RepositorySolutionAccessorFactory repositorySolutionAccessorFactory, ILogger logger) : IValidationRuleFixer<CentralPackageManagerEnabledValidationRule.Arguments>
{
    public void Fix(CentralPackageManagerEnabledValidationRule.Arguments rule, IGithubRepositoryAccessor githubRepository)
    {
        rule.ThrowIfNull();
        githubRepository.ThrowIfNull();

        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(githubRepository);
        string solutionPath = githubRepository.GetSolutionFilePath();
        DotnetSolutionModifier solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);
        // TODO: remove duplicate
        // TODO: investigate possible different versions
        IReadOnlyCollection<NugetVersion> nugetPackages = CollectNugetIncludes(solutionModifier);
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
}