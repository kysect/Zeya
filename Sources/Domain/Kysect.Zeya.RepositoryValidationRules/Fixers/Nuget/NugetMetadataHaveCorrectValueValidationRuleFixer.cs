using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.Nuget;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidationRules.Fixers.Nuget;

public class NugetMetadataHaveCorrectValueValidationRuleFixer(
    XmlDocumentSyntaxFormatter formatter,
    ILogger<NugetMetadataHaveCorrectValueValidationRuleFixer> logger) : IValidationRuleFixer<NugetMetadataHaveCorrectValueValidationRule.Arguments>
{
    public void Fix(NugetMetadataHaveCorrectValueValidationRule.Arguments rule, ILocalRepository localRepository)
    {
        rule.ThrowIfNull();
        localRepository.ThrowIfNull();

        LocalRepositorySolution repositorySolutionAccessor = localRepository.SolutionManager.GetSolution();
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();

        logger.LogTrace("Apply changes to {FileName} file", SolutionItemNameConstants.DirectoryBuildProps);

        DirectoryBuildPropsFile directoryBuildPropsFile = solutionModifier.GetOrCreateDirectoryBuildPropsModifier();
        foreach ((string key, string value) in rule.RequiredKeyValues)
        {
            logger.LogDebug("Set {Key} to {Value}", key, value);
            directoryBuildPropsFile.File.Properties.SetProperty(key, value);
        }

        // TODO: force somehow saving
        solutionModifier.Save(formatter);
    }
}