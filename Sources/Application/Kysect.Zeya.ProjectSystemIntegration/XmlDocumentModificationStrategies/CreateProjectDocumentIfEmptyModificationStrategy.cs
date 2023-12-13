using Kysect.CommonLib.BaseTypes.Extensions;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlDocumentModificationStrategies;

public class CreateProjectDocumentIfEmptyModificationStrategy : IXmlDocumentModificationStrategy
{
    public static CreateProjectDocumentIfEmptyModificationStrategy Instance { get; } = new CreateProjectDocumentIfEmptyModificationStrategy();

    public XmlDocumentSyntax Modify(XmlDocumentSyntax document)
    {
        document.ThrowIfNull();

        if (document.RootSyntax is not null)
            return document;

        const string contentTemplate =
            """
            <Project>
            </Project>
            """;

        return Parser.ParseText(contentTemplate);
    }
}