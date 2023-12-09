using System.Collections.Generic;
using System.Linq;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Tools;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;

public class DirectoryPackagePropsNugetVersionAppender : IXmlProjectFileModifyStrategy<XmlElementSyntax>
{
    private readonly IReadOnlyCollection<NugetVersion> _nugetVersions;

    public DirectoryPackagePropsNugetVersionAppender(IReadOnlyCollection<NugetVersion> nugetVersions)
    {
        _nugetVersions = nugetVersions;
    }

    public IReadOnlyCollection<XmlElementSyntax> SelectNodeForModify(XmlDocumentSyntax document)
    {
        var selectNodeForModify = document
            .GetNodesByName("ItemGroup")
            .OfType<XmlElementSyntax>()
            .Take(1)
            .ToList();
        return selectNodeForModify;
    }

    public SyntaxNode ApplyChanges(XmlElementSyntax syntax)
    {
        foreach (var (packageName, version) in _nugetVersions)
        {
            var xmlEmptyElementSyntax = ExtendedSyntaxFactory.XmlEmptyElement(
                "PackageVersion",
                new[] { ExtendedSyntaxFactory.XmlAttribute("Include", packageName), ExtendedSyntaxFactory.XmlAttribute("Version", version) });

            syntax = (XmlElementSyntax)syntax.AddChild(xmlEmptyElementSyntax);
        }

        return syntax;
    }
}