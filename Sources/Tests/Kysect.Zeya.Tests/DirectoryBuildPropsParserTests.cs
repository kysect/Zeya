using FluentAssertions;
using Kysect.Zeya.ProjectSystemIntegration;

namespace Kysect.Zeya.Tests;

public class DirectoryBuildPropsParserTests
{
    [Test]
    public void Parse_ReturnExpectedResult()
    {
        var content = """
                      <Project>
                          <PropertyGroup>
                              <Authors>Kysect</Authors>
                      		<Company>Kysect</Company>
                              <PackageReadmeFile>README.md</PackageReadmeFile>
                      		<PackageLicenseFile>LICENSE</PackageLicenseFile>
                              <LangVersion>latest</LangVersion>
                              <ImplicitUsings>enable</ImplicitUsings>
                      
                      	    <RepositoryUrl>https://github.com/kysect/Kysect.CommonLib</RepositoryUrl>
                      	    <Version>0.1.10</Version>
                          </PropertyGroup>
                      </Project>
                      """;

        var parser = new DirectoryBuildPropsParser();

        var result = parser.Parse(content);
        
        result.Keys.Should().HaveCount(8);
        result.First().Should().Be(new KeyValuePair<string, string>("Authors", "Kysect"));
    }

    [Test]
    public void GetListOfPackageReference_ReturnExpectedValue()
    {
        var content = """
                      <Project>
                          <PropertyGroup>
                              <Authors>Kysect</Authors>
                              <Company>Kysect</Company>
                              <PackageReadmeFile>Readme.md</PackageReadmeFile>
                              <PackageLicenseFile>LICENSE</PackageLicenseFile>
                          </PropertyGroup>
                      
                          <PropertyGroup>
                              <PackageReference Include="Kysect.Editorconfig" />
                              <LangVersion>latest</LangVersion>
                              <ImplicitUsings>enable</ImplicitUsings>
                              <UseArtifactsOutput>true</UseArtifactsOutput>
                          </PropertyGroup>
                      </Project>
                      """;

        var parser = new DirectoryBuildPropsParser();

        var result = parser.GetListOfPackageReference(content);
        result.Should().HaveCount(1);
        result.Single().Should().Be("Kysect.Editorconfig");
    }
}