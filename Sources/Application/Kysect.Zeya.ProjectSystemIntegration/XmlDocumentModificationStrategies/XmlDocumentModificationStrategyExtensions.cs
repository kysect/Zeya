using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetSlnParser.Parsers;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlDocumentModificationStrategies;

public static class XmlDocumentModificationStrategyExtensions
{
    public static void UpdateDocument(this XmlProjectFileAccessor projectFileAccessor, IXmlDocumentModificationStrategy modificationStrategy)
    {
        projectFileAccessor.ThrowIfNull();
        modificationStrategy.ThrowIfNull();

        projectFileAccessor.UpdateDocument(modificationStrategy.Modify);
    }
}