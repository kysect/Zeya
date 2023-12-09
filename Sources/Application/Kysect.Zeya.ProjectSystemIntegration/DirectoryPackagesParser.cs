using Microsoft.Language.Xml;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.ProjectSystemIntegration;

public class DirectoryPackagesParser
{
    public IReadOnlyCollection<NugetVersion> Parse(string content)
    {
        var root = Parser.ParseText(content);
        var versions = root
            .Descendants()
            .Where(n => n.Name == "PackageVersion")
            .Select(Parse)
            .ToList();

        return versions;
    }

    private NugetVersion Parse(IXmlElementSyntax syntax)
    {
        return new NugetVersion(
            syntax.GetAttributeValue("Include"),
            syntax.GetAttributeValue("Version"));

    }
}