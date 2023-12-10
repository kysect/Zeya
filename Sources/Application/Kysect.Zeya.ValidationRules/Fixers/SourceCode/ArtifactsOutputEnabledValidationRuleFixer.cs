using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Microsoft.Extensions.Logging;
using Microsoft.Language.Xml;
using System.IO.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class ArtifactsOutputEnabledValidationRuleFixer(IFileSystem fileSystem, ILogger logger) : IValidationRuleFixer
{
    public string DiagnosticCode => RuleDescription.SourceCode.ArtifactsOutputEnables;
    public void Fix(IGithubRepositoryAccessor githubRepository)
    {
        var solutionPath = githubRepository.GetSolutionFilePath();
        var solutionModifier = DotnetSolutionModifier.Create(solutionPath, fileSystem, logger, new SolutionFileParser(logger));

        logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryBuildPropsFileName);
        solutionModifier.DirectoryBuildPropsModifier.Accessor.UpdateDocument(AddEmptyProjectNode);
        solutionModifier.DirectoryBuildPropsModifier.Accessor.UpdateDocument(AddPropertyGroupNodeIfNotExistsModificationStrategy.Instance);
        solutionModifier.DirectoryBuildPropsModifier.Accessor.UpdateDocument(AddUseArtifactsOutputModificationStrategy.Instance);
        solutionModifier.Save();
        // TODO: force somehow saving
    }

    public XmlDocumentSyntax AddEmptyProjectNode(XmlDocumentSyntax syntax)
    {
        if (syntax.RootSyntax is not null)
            return syntax;

        var contentTemplate = """
                              <Project>
                              </Project>
                              """;

        return Parser.ParseText(contentTemplate);
    }
}