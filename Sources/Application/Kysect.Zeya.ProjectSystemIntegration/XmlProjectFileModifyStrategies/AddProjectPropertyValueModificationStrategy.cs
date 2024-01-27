using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Language.Xml;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;

public class AddProjectPropertyValueModificationStrategy(string key, string value) : IXmlProjectFileModifyStrategy<XmlElementSyntax>
{
    public IReadOnlyCollection<XmlElementSyntax> SelectNodeForModify(XmlDocumentSyntax document)
    {
        return document.GetNodesByName(key).OfType<XmlElementSyntax>().ToList();
    }

    public SyntaxNode ApplyChanges(XmlElementSyntax syntax)
    {
        syntax.ThrowIfNull();

        return syntax.WithContent(SyntaxFactory.List(ExtendedSyntaxFactory2.XmlName(value)));
    }
}