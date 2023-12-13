using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Tools;
using Kysect.Zeya.Abstractions;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Language.Xml;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;

public class AddPackageReferenceModificationStrategy(string packageName) : IXmlProjectFileModifyStrategy<XmlElementSyntax>
{
    public IReadOnlyCollection<XmlElementSyntax> SelectNodeForModify(XmlDocumentSyntax document)
    {
        var selectNodeForModify = document
            .GetNodesByName("ItemGroup")
            .OfType<XmlElementSyntax>()
            .Take(1)
            .ToList();

        if (selectNodeForModify.IsEmpty())
            throw new ZeyaException("Node ItemGroup was not found");

        return selectNodeForModify;
    }

    public SyntaxNode ApplyChanges(XmlElementSyntax syntax)
    {
        var packageAttribute = ExtendedSyntaxFactory
                .XmlEmptyElement("PackageReference", 2)
                .AddAttribute(ExtendedSyntaxFactory.XmlAttribute("Include", packageName));

        return syntax.AddChild(packageAttribute).To<XmlElementSyntax>();
    }
}