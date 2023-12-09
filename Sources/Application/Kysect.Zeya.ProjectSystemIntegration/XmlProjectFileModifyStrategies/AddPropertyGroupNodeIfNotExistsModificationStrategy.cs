using System.Collections.Generic;
using System.Linq;
using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Tools;
using Kysect.Zeya.Abstractions;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;

public class AddPropertyGroupNodeIfNotExistsModificationStrategy : IXmlProjectFileModifyStrategy<XmlElementSyntax>
{
    public static AddPropertyGroupNodeIfNotExistsModificationStrategy Instance { get; } = new AddPropertyGroupNodeIfNotExistsModificationStrategy();

    public IReadOnlyCollection<XmlElementSyntax> SelectNodeForModify(XmlDocumentSyntax document)
    {
        var selectNodeForModify = document
            .GetNodesByName("Project")
            .OfType<XmlElementSyntax>()
            .ToList();

        if (selectNodeForModify.Count != 1)
            throw new ZeyaException($"Unexpected Project node count in xml file: {selectNodeForModify.Count}");

        return selectNodeForModify;
    }

    public SyntaxNode ApplyChanges(XmlElementSyntax syntax)
    {
        var propertyGroupExists = syntax
            .ChildNodes
            .OfType<XmlElementSyntax>()
            .Any(c => c.Name == "PropertyGroup");

        if (propertyGroupExists)
            return syntax;

        return syntax
            .AddChild(ExtendedSyntaxFactory.XmlElement("PropertyGroup", 1))
            .To<XmlElementSyntax>();
    }
}