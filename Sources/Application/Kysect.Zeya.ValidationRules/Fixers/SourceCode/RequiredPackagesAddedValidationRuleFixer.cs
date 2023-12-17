using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.ProjectSystemIntegration.XmlDocumentModificationStrategies;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class RequiredPackagesAddedValidationRuleFixer(DotnetSolutionModifierFactory dotnetSolutionModifierFactory, RepositorySolutionAccessorFactory repositorySolutionAccessorFactory, ILogger logger) : IValidationRuleFixer<RequiredPackagesAddedValidationRule.Arguments>
{
    public void Fix(RequiredPackagesAddedValidationRule.Arguments rule, IGithubRepositoryAccessor githubRepository)
    {
        rule.ThrowIfNull();
        githubRepository.ThrowIfNull();

        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(githubRepository);
        var solutionPath = githubRepository.GetSolutionFilePath();
        var solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);
        string directoryPackagePropsPath = repositorySolutionAccessor.GetDirectoryPackagePropsPath();

        logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryBuildPropsFileName);
        solutionModifier.DirectoryBuildPropsModifier.Accessor.UpdateDocument(CreateProjectDocumentIfEmptyModificationStrategy.Instance);
        solutionModifier.DirectoryBuildPropsModifier.Accessor.UpdateDocument(AddProjectGroupNodeIfNotExistsModificationStrategy.ItemGroup);

        foreach (var rulePackage in rule.Packages)
        {
            logger.LogDebug("Adding package {Package} to {DirectoryBuildFile}", rulePackage, directoryPackagePropsPath);
            solutionModifier.DirectoryBuildPropsModifier.Accessor.UpdateDocument(new AddPackageReferenceModificationStrategy(rulePackage));
        }

        logger.LogTrace("Saving solution files");
        solutionModifier.Save();
    }
}