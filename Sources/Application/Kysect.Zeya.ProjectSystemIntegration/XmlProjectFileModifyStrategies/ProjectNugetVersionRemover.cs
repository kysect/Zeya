using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Tools;
using Microsoft.Language.Xml;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;

public class ProjectNugetVersionRemover : IXmlProjectFileModifyStrategy<SyntaxNode>
{
    public static ProjectNugetVersionRemover Instance { get; } = new ProjectNugetVersionRemover();

    public IReadOnlyCollection<SyntaxNode> SelectNodeForModify(XmlDocumentSyntax document)
    {
        return document
            .GetNodesByName("PackageReference")
            .OfType<SyntaxNode>()
            .ToList();
    }

    public SyntaxNode ApplyChanges(SyntaxNode syntax)
    {
        if (syntax is not IXmlElementSyntax xmlElementSyntax)
            return syntax;

        var versionAttribute = xmlElementSyntax.GetAttribute("Version");
        if (versionAttribute is not null)
            xmlElementSyntax = xmlElementSyntax.RemoveAttribute(versionAttribute);

        return xmlElementSyntax.AsNode;
    }
}