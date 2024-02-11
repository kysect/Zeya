using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.ValidationRules.Abstractions;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class RequiredPackagesAddedValidationRuleFixer(
    RepositorySolutionAccessorFactory repositorySolutionAccessorFactory,
    XmlDocumentSyntaxFormatter formatter,
    ILogger logger)
    : IValidationRuleFixer<RequiredPackagesAddedValidationRule.Arguments>
{
    public void Fix(RequiredPackagesAddedValidationRule.Arguments rule, IClonedRepository clonedRepository)
    {
        rule.ThrowIfNull();
        clonedRepository.ThrowIfNull();

        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(clonedRepository);
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();
        DirectoryBuildPropsFile directoryBuildPropsFile = solutionModifier.GetOrCreateDirectoryBuildPropsModifier();
        var directoryPackageProps = solutionModifier.GetOrCreateDirectoryPackagePropsModifier();

        logger.LogTrace("Apply changes to {FileName} file", SolutionItemNameConstants.DirectoryBuildProps);
        HashSet<string> addedPackage = directoryBuildPropsFile
            .File
            .PackageReferences
            .GetPackageReferences()
            .Select(p => p.Name)
            .ToHashSet();

        foreach (ProjectPackageVersion rulePackage in rule.Packages)
        {
            if (addedPackage.Contains(rulePackage.Name))
                continue;

            logger.LogDebug("Adding package {Package} to {DirectoryBuildFile}", rulePackage.Name, SolutionItemNameConstants.DirectoryBuildProps);
            directoryBuildPropsFile.File.PackageReferences.AddPackageReference(rulePackage.Name);

            logger.LogDebug("Adding package {Package} version {Verion} to {DirectoryPackageFile}", rulePackage.Name, rulePackage.Version, SolutionItemNameConstants.DirectoryBuildProps);
            directoryPackageProps.Versions.AddPackageVersion(rulePackage.Name, rulePackage.Version);

            logger.LogDebug("Removing package {Package} from csproj files", rulePackage.Name);
            foreach (var dotnetProjectModifier in solutionModifier.Projects)
                dotnetProjectModifier.Value.File.PackageReferences.RemovePackageReference(rulePackage.Name);
        }

        logger.LogTrace("Saving solution files");
        solutionModifier.Save(formatter);
    }
}