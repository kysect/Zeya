using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Kysect.Zeya.ValidationRules.Rules.Nuget;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.Nuget;

public class NugetMetadataHaveCorrectValueValidationRuleFixer(DotnetSolutionModifierFactory dotnetSolutionModifierFactory, ILogger logger) : IValidationRuleFixer<NugetMetadataHaveCorrectValueValidationRule.Arguments>
{
    public void Fix(NugetMetadataHaveCorrectValueValidationRule.Arguments rule, IGithubRepositoryAccessor githubRepository)
    {
        rule.ThrowIfNull();
        githubRepository.ThrowIfNull();

        var solutionPath = githubRepository.GetSolutionFilePath();
        var solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);

        logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryBuildPropsFileName);

        var projectPropertyModifier = new ProjectPropertyModifier(solutionModifier.DirectoryBuildPropsModifier.Accessor, logger);
        foreach (var (key, value) in rule.RequiredKeyValues)
        {
            logger.LogDebug("Set {Key} to {Value}", key, value);
            projectPropertyModifier.AddOrUpdateProperty(key, value);
        }

        // TODO: force somehow saving
        solutionModifier.Save();
    }
}