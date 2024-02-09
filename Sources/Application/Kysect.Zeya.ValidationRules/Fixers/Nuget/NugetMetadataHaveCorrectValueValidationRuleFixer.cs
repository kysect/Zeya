using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.RepositoryAccess;
using Kysect.Zeya.ValidationRules.Abstractions;
using Kysect.Zeya.ValidationRules.Rules.Nuget;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.Nuget;

public class NugetMetadataHaveCorrectValueValidationRuleFixer(
    RepositorySolutionAccessorFactory repositorySolutionAccessorFactory,
    XmlDocumentSyntaxFormatter formatter,
    ILogger logger) : IValidationRuleFixer<NugetMetadataHaveCorrectValueValidationRule.Arguments>
{
    public void Fix(NugetMetadataHaveCorrectValueValidationRule.Arguments rule, IClonedRepository clonedRepository)
    {
        rule.ThrowIfNull();
        clonedRepository.ThrowIfNull();

        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(clonedRepository);
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