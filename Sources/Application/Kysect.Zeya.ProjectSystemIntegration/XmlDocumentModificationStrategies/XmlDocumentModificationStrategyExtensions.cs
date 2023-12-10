using Kysect.DotnetSlnParser.Parsers;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlDocumentModificationStrategies;

public static class XmlDocumentModificationStrategyExtensions
{
    public static void UpdateDocument(this XmlProjectFileAccessor projectFileAccessor, IXmlDocumentModificationStrategy modificationStrategy)
    {
        projectFileAccessor.UpdateDocument(modificationStrategy.Modify);
    }
}