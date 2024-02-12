using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.ValidationRules.Abstractions;
using Kysect.Zeya.ValidationRules.Rules.Nuget;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.Nuget;

public class NugetMetadataHaveCorrectValueValidationRuleFixer(
    XmlDocumentSyntaxFormatter formatter,
    ILogger logger) : IValidationRuleFixer<NugetMetadataHaveCorrectValueValidationRule.Arguments>
{
    public void Fix(NugetMetadataHaveCorrectValueValidationRule.Arguments rule, IClonedRepository clonedRepository)
    {
        rule.ThrowIfNull();
        clonedRepository.ThrowIfNull();

        LocalRepositorySolution repositorySolutionAccessor = clonedRepository.SolutionManager.GetSolution();
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