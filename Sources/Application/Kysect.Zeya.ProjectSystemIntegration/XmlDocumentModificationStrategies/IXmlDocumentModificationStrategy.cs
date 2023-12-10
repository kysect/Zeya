using Microsoft.Language.Xml;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlDocumentModificationStrategies;

public interface IXmlDocumentModificationStrategy
{
    XmlDocumentSyntax Modify(XmlDocumentSyntax document);
}