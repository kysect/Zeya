using Microsoft.Extensions.Logging;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Kysect.Zeya.ProjectSystemIntegration;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class ArtifactsOutputEnabledValidationRuleFixer(DotnetSolutionModifierFactory dotnetSolutionModifierFactory, ILogger logger) : IValidationRuleFixer<ArtifactsOutputEnabledValidationRule.Arguments>
{
    public void Fix(ArtifactsOutputEnabledValidationRule.Arguments rule, IGithubRepositoryAccessor githubRepository)
    {
        var solutionPath = githubRepository.GetSolutionFilePath();
        var solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);

        logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryBuildPropsFileName);

        var projectPropertyModifier = new ProjectPropertyModifier(solutionModifier.DirectoryBuildPropsModifier.Accessor, logger);
        projectPropertyModifier.AddOrUpdateProperty("UseArtifactsOutput", "true");

        // TODO: force somehow saving
        solutionModifier.Save();
    }
}