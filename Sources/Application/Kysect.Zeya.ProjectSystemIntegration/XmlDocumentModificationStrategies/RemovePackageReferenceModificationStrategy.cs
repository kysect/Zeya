using Kysect.DotnetSlnParser.Tools;
using Microsoft.Language.Xml;
using System.Linq;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlDocumentModificationStrategies;

public class RemovePackageReferenceModificationStrategy(string packageName) : IXmlDocumentModificationStrategy
{
    public XmlDocumentSyntax Modify(XmlDocumentSyntax document)
    {
        var includes = document
            .GetNodesByName("PackageReference")
            .Where(RequiredPackageReference)
            .Select(n => n.AsNode)
            .ToList();

        return document.RemoveNodes(includes, SyntaxRemoveOptions.KeepNoTrivia);
    }

    private bool RequiredPackageReference(IXmlElementSyntax xmlElementSyntax)
    {
        XmlAttributeSyntax includeAttribute = xmlElementSyntax.GetAttribute("Include");
        if (includeAttribute is null)
            return false;

        return includeAttribute.Value.Trim('"') == packageName;
    }
}