using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.Abstractions;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Language.Xml;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;

public class DirectoryPackagePropsNugetVersionAppender : IXmlProjectFileModifyStrategy<XmlElementSyntax>
{
    private readonly IReadOnlyCollection<ProjectPackageVersion> _nugetVersions;

    public DirectoryPackagePropsNugetVersionAppender(IReadOnlyCollection<ProjectPackageVersion> nugetVersions)
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

        if (selectNodeForModify.IsEmpty())
            throw new ZeyaException("Node ItemGroup was not found");

        return selectNodeForModify;
    }

    public SyntaxNode ApplyChanges(XmlElementSyntax syntax)
    {
        foreach (var (packageName, version) in _nugetVersions)
        {
            var xmlEmptyElementSyntax = ExtendedSyntaxFactory2
                .XmlEmptyElement("PackageVersion", 2)
                .AddAttribute(ExtendedSyntaxFactory2.XmlAttribute("Include", packageName))
                .AddAttribute(ExtendedSyntaxFactory2.XmlAttribute("Version", version));

            syntax = (XmlElementSyntax) syntax.AddChild(xmlEmptyElementSyntax);
        }

        return syntax;
    }
}