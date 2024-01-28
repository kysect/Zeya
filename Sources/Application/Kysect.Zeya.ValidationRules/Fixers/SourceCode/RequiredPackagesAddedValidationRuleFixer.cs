using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.Abstractions.Contracts;
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

        logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryBuildPropsFileName);
        HashSet<string> addedPackage = directoryBuildPropsFile
            .File
            .PackageReferences
            .GetPackageReferences()
            .Select(p => p.Name)
            .ToHashSet();

        foreach (var rulePackage in rule.Packages)
        {
            if (addedPackage.Contains(rulePackage))
                continue;

            logger.LogDebug("Adding package {Package} to {DirectoryBuildFile}", rulePackage, ValidationConstants.DirectoryBuildPropsFileName);
            directoryBuildPropsFile.File.PackageReferences.AddPackageReference(rulePackage);

            logger.LogDebug("Removing package {Package} from csproj files", rulePackage);
            foreach (var dotnetProjectModifier in solutionModifier.Projects)
                dotnetProjectModifier.Value.File.PackageReferences.RemovePackageReference(rulePackage);
        }

        logger.LogTrace("Saving solution files");
        solutionModifier.Save(formatter);
    }
}