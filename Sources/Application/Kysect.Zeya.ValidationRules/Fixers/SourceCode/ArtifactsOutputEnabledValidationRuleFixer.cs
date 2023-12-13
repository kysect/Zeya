using Microsoft.Extensions.Logging;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.CommonLib.BaseTypes.Extensions;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class ArtifactsOutputEnabledValidationRuleFixer(DotnetSolutionModifierFactory dotnetSolutionModifierFactory, ILogger logger) : IValidationRuleFixer<ArtifactsOutputEnabledValidationRule.Arguments>
{
    public void Fix(ArtifactsOutputEnabledValidationRule.Arguments rule, IGithubRepositoryAccessor githubRepository)
    {
        rule.ThrowIfNull();
        githubRepository.ThrowIfNull();

        var solutionPath = githubRepository.GetSolutionFilePath();
        var solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);

        logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryBuildPropsFileName);

        var projectPropertyModifier = new ProjectPropertyModifier(solutionModifier.DirectoryBuildPropsModifier.Accessor, logger);
        logger.LogDebug("Set UseArtifactsOutput to true");
        projectPropertyModifier.AddOrUpdateProperty("UseArtifactsOutput", "true");

        // TODO: force somehow saving
        solutionModifier.Save();
    }
}