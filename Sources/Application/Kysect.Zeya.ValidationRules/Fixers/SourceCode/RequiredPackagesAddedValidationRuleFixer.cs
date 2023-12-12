using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;
using Kysect.Zeya.ProjectSystemIntegration.XmlDocumentModificationStrategies;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class RequiredPackagesAddedValidationRuleFixer(DotnetSolutionModifierFactory dotnetSolutionModifierFactory, ILogger logger) : IValidationRuleFixer<RequiredPackagesAddedValidationRule.Argument>
{
    public void Fix(RequiredPackagesAddedValidationRule.Argument rule, IGithubRepositoryAccessor githubRepository)
    {
        var solutionPath = githubRepository.GetSolutionFilePath();
        var solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);

        logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryBuildPropsFileName);
        solutionModifier.DirectoryBuildPropsModifier.Accessor.UpdateDocument(CreateProjectDocumentIfEmptyModificationStrategy.Instance);
        solutionModifier.DirectoryBuildPropsModifier.Accessor.UpdateDocument(AddProjectGroupNodeIfNotExistsModificationStrategy.ItemGroup);

        foreach (var rulePackage in rule.Packages)
        {
            logger.LogDebug("Adding package {Package} to {DirectoryBuildFile}", rulePackage, ValidationConstants.DirectoryPackagePropsFileName);
            solutionModifier.DirectoryBuildPropsModifier.Accessor.UpdateDocument(new AddPackageReferenceModificationStrategy(rulePackage));
        }

        logger.LogTrace("Saving solution files");
        solutionModifier.Save();
    }
}