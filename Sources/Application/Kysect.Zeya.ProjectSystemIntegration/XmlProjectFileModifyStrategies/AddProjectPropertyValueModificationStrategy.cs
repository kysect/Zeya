using System.Collections.Generic;
using System.Linq;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Tools;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;

public class AddProjectPropertyValueModificationStrategy(string key, string value) : IXmlProjectFileModifyStrategy<XmlElementSyntax>
{
    public IReadOnlyCollection<XmlElementSyntax> SelectNodeForModify(XmlDocumentSyntax document)
    {
        return document.GetNodesByName(key).OfType<XmlElementSyntax>().ToList();
    }

    public SyntaxNode ApplyChanges(XmlElementSyntax syntax)
    {
        return syntax.WithContent(SyntaxFactory.List(ExtendedSyntaxFactory.XmlName(value)));
    }
}