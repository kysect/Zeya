using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.Abstractions;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Language.Xml;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;

public class AddProjectGroupNodeIfNotExistsModificationStrategy(string groupName) : IXmlProjectFileModifyStrategy<XmlElementSyntax>
{
    public static AddProjectGroupNodeIfNotExistsModificationStrategy PropertyGroup { get; } = new AddProjectGroupNodeIfNotExistsModificationStrategy("PropertyGroup");
    public static AddProjectGroupNodeIfNotExistsModificationStrategy ItemGroup { get; } = new AddProjectGroupNodeIfNotExistsModificationStrategy("ItemGroup");

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
        syntax.ThrowIfNull();

        var propertyGroupExists = syntax
            .AsSyntaxElement
            .Descendants()
            .OfType<XmlElementSyntax>()
            .Any(c => c.Name == groupName);

        if (propertyGroupExists)
            return syntax;

        return syntax
            .AddChild(ExtendedSyntaxFactory2.XmlElement(groupName, 1))
            .To<XmlElementSyntax>();
    }
}