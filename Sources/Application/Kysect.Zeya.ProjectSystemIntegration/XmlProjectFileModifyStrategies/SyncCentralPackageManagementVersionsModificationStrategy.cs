using Kysect.CommonLib.BaseTypes.Extensions;
using System.Collections.Generic;
using System.Linq;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Tools;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;

public class SyncCentralPackageManagementVersionsModificationStrategy(Dictionary<string, string> versions) : IXmlProjectFileModifyStrategy<XmlEmptyElementSyntax>
{
    public IReadOnlyCollection<XmlEmptyElementSyntax> SelectNodeForModify(XmlDocumentSyntax document)
    {
        var specifiedPackages = versions.Keys.ToHashSet();

        var packageNodes = document
            .GetNodesByName("PackageVersion")
            .Where(n => n.Attributes.Any(a => specifiedPackages.Contains(a.Value)))
            .OfType<XmlEmptyElementSyntax>()
            .ToList();

        return packageNodes;
    }

    public SyntaxNode ApplyChanges(XmlEmptyElementSyntax syntax)
    {
        syntax.ThrowIfNull();

        var packageName = syntax.GetAttribute("Include").Value;

        var oldVersionAttribute = syntax.GetAttribute("Version");

        return syntax.ReplaceNode(oldVersionAttribute, ExtendedSyntaxFactory.XmlAttribute("Version", versions[packageName]));
    }
}