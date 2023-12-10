using System.Collections.Generic;
using System.Linq;
using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Tools;
using Kysect.Zeya.Abstractions;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;

public class UpdateProjectPropertyValueModificationStrategy(string key, string value) : IXmlProjectFileModifyStrategy<XmlElementSyntax>
{
    public IReadOnlyCollection<XmlElementSyntax> SelectNodeForModify(XmlDocumentSyntax document)
    {
        var propertyGroups = document.GetNodesByName("PropertyGroup").OfType<XmlElementSyntax>().ToList();
        if (propertyGroups.IsEmpty())
            throw new ZeyaException("Unexpected missed PropertyGroup node");

        return propertyGroups.Take(1).ToList();
    }

    public SyntaxNode ApplyChanges(XmlElementSyntax syntax)
    {
        return syntax
            .AddChild(ExtendedSyntaxFactory.PropertyGroupParameter(key, value))
            .To<XmlElementSyntax>();
    }
}