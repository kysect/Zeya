﻿using Microsoft.Language.Xml;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.ProjectSystemIntegration;

public class DirectoryBuildPropsParser
{
    public Dictionary<string, string> Parse(string content)
    {
        var root = Parser.ParseText(content);
        var propertyNodes = root
            .Descendants()
            .Where(n => n.Name == "PropertyGroup")
            .SelectMany(n => n.Elements)
            .ToList();

        Dictionary<string, string> result = new Dictionary<string, string>();
        foreach (var xmlElementSyntax in propertyNodes)
        {
            var nodeContent = xmlElementSyntax.Content.FirstOrDefault();
            if (nodeContent is not null)
            {
                result[xmlElementSyntax.Name] = nodeContent.ToFullString();
            }
        }

        return result;
    }

    public IReadOnlyCollection<string> GetListOfPackageReference(string content)
    {
        var root = Parser.ParseText(content);
        var propertyNodes = root
            .Descendants()
            .Where(n => n.Name == "PackageReference")
            .ToList();

        var result = new List<string>();
        foreach (var xmlElementSyntax in propertyNodes)
        {
            var packageName = xmlElementSyntax.GetAttribute("Include");
            result.Add(packageName.Value);
        }

        return result;
    }
}