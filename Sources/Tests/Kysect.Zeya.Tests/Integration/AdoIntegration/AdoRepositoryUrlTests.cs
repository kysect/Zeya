using FluentAssertions;
using Kysect.Zeya.AdoIntegration.Abstraction;

namespace Kysect.Zeya.Tests.Integration.AdoIntegration;

public class AdoRepositoryUrlTests
{
    [Fact]
    public void Parse_ForFullUrlString_ReturnExpectedParts()
    {
        AdoRepositoryUrl expected = new("https://dev.azure.com/SomeOrg", "SomeProject", "RepoName");
        const string input = "https://dev.azure.com/SomeOrg/SomeProject/_git/RepoName";

        AdoRepositoryUrl adoRepositoryUrl = AdoRepositoryUrl.Parse(input);

        adoRepositoryUrl.Should().Be(expected);
    }
}