using FluentAssertions;
using Kysect.Zeya.AdoIntegration.Abstraction;

namespace Kysect.Zeya.Tests.Integration.AdoIntegration;

public class AdoRepositoryUrlTests
{
    [Fact]
    public void Serialize_ForSimpleInstance_ReturnExpectedValue()
    {
        AdoRepositoryUrl value = new("SomeCollection", "SomeProject", "RepoName");
        const string expected = "SomeCollection/SomeProject/RepoName";

        value.Serialize().Should().Be(expected);
    }

    [Fact]
    public void ToFullLink_ForSimpleInstance_ReturnExpectedValue()
    {
        AdoRepositoryUrl value = new("SomeCollection", "SomeProject", "RepoName");
        string hostUrl = "https://dev.azure.com/";
        const string expected = "https://dev.azure.com/SomeCollection/SomeProject/_git/RepoName";

        value.ToFullLink(hostUrl).Should().Be(expected);
    }

    [Fact]
    public void Parse_ForSimpleInstance_ReturnExpectedValue()
    {
        AdoRepositoryUrl expected = new("SomeCollection", "SomeProject", "RepoName");
        const string input = "SomeCollection/SomeProject/RepoName";

        AdoRepositoryUrl actual = AdoRepositoryUrl.Parse(input);

        actual.Should().Be(expected);
    }
}