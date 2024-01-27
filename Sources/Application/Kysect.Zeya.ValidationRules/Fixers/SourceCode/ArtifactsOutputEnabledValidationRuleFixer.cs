using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class ArtifactsOutputEnabledValidationRuleFixer(
    DotnetSolutionModifierFactory dotnetSolutionModifierFactory,
    RepositorySolutionAccessorFactory repositorySolutionAccessorFactory,
    XmlDocumentSyntaxFormatter formatter,
    ILogger logger) : IValidationRuleFixer<ArtifactsOutputEnabledValidationRule.Arguments>
{
    public void Fix(ArtifactsOutputEnabledValidationRule.Arguments rule, IClonedRepository clonedRepository)
    {
        rule.ThrowIfNull();
        clonedRepository.ThrowIfNull();

        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(clonedRepository);
        string solutionPath = repositorySolutionAccessor.GetSolutionFilePath();
        DotnetSolutionModifier solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);

        logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryBuildPropsFileName);

        var projectPropertyModifier = new ProjectPropertyModifier(solutionModifier.GetOrCreateDirectoryBuildPropsModifier().File, logger);
        logger.LogDebug("Set UseArtifactsOutput to true");
        projectPropertyModifier.AddOrUpdateProperty("UseArtifactsOutput", "true");

        // TODO: force somehow saving
        solutionModifier.Save(formatter);
    }
}