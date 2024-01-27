using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration.XmlDocumentModificationStrategies;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class RequiredPackagesAddedValidationRuleFixer(
    DotnetSolutionModifierFactory dotnetSolutionModifierFactory,
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
        string solutionPath = repositorySolutionAccessor.GetSolutionFilePath();

        var solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);
        DirectoryBuildPropsFile directoryBuildPropsFile = solutionModifier.GetOrCreateDirectoryBuildPropsModifier();

        string directoryPackagePropsPath = repositorySolutionAccessor.GetDirectoryPackagePropsPath();

        logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryBuildPropsFileName);
        directoryBuildPropsFile.File.GetOrAddItemGroup();

        foreach (var rulePackage in rule.Packages)
        {
            logger.LogDebug("Adding package {Package} to {DirectoryBuildFile}", rulePackage, directoryPackagePropsPath);
            directoryBuildPropsFile.File.UpdateDocument(new AddPackageReferenceModificationStrategy(rulePackage));

            logger.LogDebug("Removing package {Package} from csproj files", rulePackage);
            foreach (DotnetProjectModifier dotnetProjectModifier in solutionModifier.Projects)
                dotnetProjectModifier.File.UpdateDocument(new RemovePackageReferenceModificationStrategy(rulePackage));
        }

        logger.LogTrace("Saving solution files");
        solutionModifier.Save(formatter);
    }
}