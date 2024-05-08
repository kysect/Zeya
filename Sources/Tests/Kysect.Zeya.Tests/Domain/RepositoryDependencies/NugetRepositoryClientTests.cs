using FluentAssertions;
using Kysect.Zeya.RepositoryDependencies;

namespace Kysect.Zeya.Tests.Domain.RepositoryDependencies;

public class NugetRepositoryClientTests
{
    [Fact]
    public async Task GetDependencies_DotnetProjectSystem_ReturnCorrectDependencyList()
    {
        using var nugetRepositoryClient = new NugetRepositoryClient();
        string[] expected = ["GuiLabs.Language.Xml", "Kysect.CommonLib", "Kysect.Editorconfig"];

        IReadOnlyCollection<string> dependencies = await nugetRepositoryClient.GetDependencies("Kysect.DotnetProjectSystem");

        dependencies.Should().BeEquivalentTo(expected);
    }
}