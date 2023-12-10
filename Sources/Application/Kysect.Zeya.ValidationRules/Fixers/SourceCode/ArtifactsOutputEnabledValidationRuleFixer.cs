using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Parsers;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class ArtifactsOutputEnabledValidationRuleFixer(IFileSystem fileSystem, ILogger logger) : IValidationRuleFixer<ArtifactsOutputEnabledValidationRule.Arguments>
{
    public string DiagnosticCode => RuleDescription.SourceCode.ArtifactsOutputEnables;
    public void Fix(ArtifactsOutputEnabledValidationRule.Arguments rule, IGithubRepositoryAccessor githubRepository)
    {
        var solutionPath = githubRepository.GetSolutionFilePath();
        var solutionModifier = DotnetSolutionModifier.Create(solutionPath, fileSystem, logger, new SolutionFileParser(logger));

        logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryBuildPropsFileName);

        var projectPropertyModifier = new ProjectPropertyModifier(solutionModifier.DirectoryBuildPropsModifier.Accessor, logger);
        projectPropertyModifier.AddOrUpdateProperty("UseArtifactsOutput", "true");
        
        // TODO: force somehow saving
        solutionModifier.Save();
    }
}