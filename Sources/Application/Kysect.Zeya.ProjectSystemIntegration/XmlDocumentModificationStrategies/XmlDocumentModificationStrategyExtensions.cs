using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlDocumentModificationStrategies;

public static class XmlDocumentModificationStrategyExtensions
{
    public static void UpdateDocument(this DotnetProjectFile projectFileAccessor, IXmlDocumentModificationStrategy modificationStrategy)
    {
        projectFileAccessor.ThrowIfNull();
        modificationStrategy.ThrowIfNull();

        projectFileAccessor.UpdateDocument(modificationStrategy.Modify);
    }
}