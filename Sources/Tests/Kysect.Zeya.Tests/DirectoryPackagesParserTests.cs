using FluentAssertions;
using Kysect.Zeya.ProjectSystemIntegration;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.Tests;

public class DirectoryPackagesParserTests
{
    [Test]
    public void Parse_ReturnInfoAboutPackages()
    {
        var content = """
                      <Project>
                        <PropertyGroup>
                          <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                        </PropertyGroup>
                        <ItemGroup>
                          <PackageVersion Include="codeessentials.Extensions.Logging.Demystifier" Version="1.1.66" />
                          <PackageVersion Include="Kysect.Editorconfig" Version="1.1.4" />
                          <PackageVersion Include="Microsoft.Extensions.Configuration" Version="7.0.0" />
                        </ItemGroup>
                      </Project>
                      """;

        var parser = new DirectoryPackagesParser();

        var result = parser.Parse(content);
        result.Should().HaveCount(3);
        result.First().Should().Be(new NugetVersion("codeessentials.Extensions.Logging.Demystifier", "1.1.66"));
    }

    [Test]
    public void Temp()
    {
        var content = """
                      <Project>
                        <PropertyGroup>
                          <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                        </PropertyGroup>
                        <ItemGroup>
                          <PackageVersion Include="codeessentials.Extensions.Logging.Demystifier" Version="1.1.66" />
                          <PackageVersion Include="Kysect.Editorconfig" Version="1.1.4" />
                          <PackageVersion Include="Microsoft.Extensions.Configuration" Version="7.0.0" />
                        </ItemGroup>
                      </Project>
                      """;

        var root = Parser.ParseText(content);
        var versions = root
            .Descendants()
            .Where(n => n.Name == "PackageVersion")
            //.Select(n => n.AsNode)
            //.OfType<XmlElementSyntax>()
            .OfType<XmlEmptyElementSyntax>()
            .ToList();
        var replaced = root.ReplaceNodes(versions, (_, n) =>
        {
            var attributeForRemove = n.GetAttribute("Version");
            var modifiedNode = n.RemoveAttribute(attributeForRemove);
            return modifiedNode.AsNode;
        });


        var replaces = replaced.ToFullString();
        return;
    }
}