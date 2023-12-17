using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Tools;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Language.Xml;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;

public class AddUseArtifactsOutputModificationStrategy : IXmlProjectFileModifyStrategy<XmlElementSyntax>
{
    public static AddUseArtifactsOutputModificationStrategy Instance { get; } = new AddUseArtifactsOutputModificationStrategy();

    public IReadOnlyCollection<XmlElementSyntax> SelectNodeForModify(XmlDocumentSyntax document)
    {
        var selectNodeForModify = document
            .GetNodesByName("PropertyGroup")
            .OfType<XmlElementSyntax>()
            .ToList();

        return selectNodeForModify.Take(1).ToList();
    }

    public SyntaxNode ApplyChanges(XmlElementSyntax syntax)
    {
        return syntax.AddChild(ExtendedSyntaxFactory.PropertyGroupParameter("UseArtifactsOutput", "true")).To<XmlElementSyntax>();
    }
}