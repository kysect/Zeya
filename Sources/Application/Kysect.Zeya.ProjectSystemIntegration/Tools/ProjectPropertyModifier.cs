using System.Linq;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.DotnetSlnParser.Tools;
using Kysect.Zeya.Abstractions;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Microsoft.Extensions.Logging;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.ProjectSystemIntegration.Tools;

public class ProjectPropertyModifier(XmlProjectFileAccessor projectFileAccessor, ILogger logger)
{
    public void AddOrUpdateProperty(string key, string value)
    {
        if (projectFileAccessor.Document.RootSyntax is null)
            projectFileAccessor.UpdateDocument(AddEmptyProjectNode);

        projectFileAccessor.UpdateDocument(AddPropertyGroupNodeIfNotExistsModificationStrategy.Instance);
        Modify(key, value);
    }

    private void Modify(string key, string value)
    {
        var propertyNodes = projectFileAccessor.GetNodesByName(key);
        if (propertyNodes.Any())
        {
            if (propertyNodes.Count > 1)
                logger.LogWarning("Found multiple nodes with name {Name}", key);

            projectFileAccessor.UpdateDocument(d =>
            {
                foreach (IXmlElementSyntax elementSyntax in propertyNodes)
                {
                    IXmlElementSyntax modifiedPropertyElement = elementSyntax.WithContent(SyntaxFactory.List(ExtendedSyntaxFactory.XmlName(value)));
                    d = d.ReplaceNode(elementSyntax.AsNode, modifiedPropertyElement.AsNode);
                }

                return d;
            });

            return;
        }

        IXmlElementSyntax propertyGroup = SelectPropertyGroupForInsert();
        var modifiedPropertyGroup = propertyGroup.AddChild(ExtendedSyntaxFactory.PropertyGroupParameter(key, value));
        projectFileAccessor.UpdateDocument(d => d.ReplaceNode(propertyGroup.AsNode, modifiedPropertyGroup.AsNode));
    }

    private IXmlElementSyntax SelectPropertyGroupForInsert()
    {
        // TODO: introduce constant PropertyGroup
        var propertyGroupNodes = projectFileAccessor.Document.GetNodesByName("PropertyGroup");
        if (propertyGroupNodes.IsEmpty())
            projectFileAccessor.UpdateDocument(AddPropertyGroupNodeIfNotExistsModificationStrategy.Instance);

        propertyGroupNodes = projectFileAccessor.Document.GetNodesByName("PropertyGroup");
        if (propertyGroupNodes.IsEmpty())
            throw new ZeyaException("Unexpected missed PropertyGroup node");

        return propertyGroupNodes.First();
    }

    private XmlDocumentSyntax AddEmptyProjectNode(XmlDocumentSyntax syntax)
    {
        var contentTemplate = """
                              <Project>
                              </Project>
                              """;

        return Parser.ParseText(contentTemplate);
    }
}