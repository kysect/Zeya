using FluentAssertions;
using Kysect.Zeya.GithubIntegration.Abstraction;

namespace Kysect.Zeya.Tests.Integration.GithubIntegration;

public class GithubRepositoryNameTests
{
    [Fact]
    public void FullName_ReturnStringInExpectedFormat()
    {
        var githubRepositoryName = new GithubRepositoryName("owner", "name");

        githubRepositoryName.FullName.Should().Be("owner/name");
    }
}