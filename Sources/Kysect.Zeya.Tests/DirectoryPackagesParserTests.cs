using FluentAssertions;
using Kysect.Zeya.ProjectSystemIntegration;

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
}