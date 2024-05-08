using FluentAssertions;
using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Graphs;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryDependencies;
using Kysect.Zeya.RepositoryDependencies.NuGetPackagesSearcher;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.RepositoryDependencies;

public class FakeNuGetPackagesSearcher : INuGetPackagesSearcher
{
    public Dictionary<string, List<string>> RepositoryProjects { get; }

    public FakeNuGetPackagesSearcher(Dictionary<string, List<string>> repositoryProjects)
    {
        RepositoryProjects = repositoryProjects;
    }

    public IReadOnlyCollection<string> GetContainingProjectNames(ILocalRepository repository)
    {
        repository.ThrowIfNull();
        return RepositoryProjects[repository.GetRepositoryName()];
    }
}

public class NugetPackageUpdateOrderBuilderTests
{
    private readonly NugetPackageUpdateOrderBuilder _nugetPackageUpdateOrderBuilder;
    private readonly Dictionary<string, List<string>> _repositoryProjects;
    private readonly LocalRepositoryProvider _localRepositoryProvider;

    public NugetPackageUpdateOrderBuilderTests()
    {
        var validationTestFixture = new ValidationTestFixture();
        _localRepositoryProvider = validationTestFixture.GetRequiredService<LocalRepositoryProvider>();

        var nugetRepositoryClient = new NugetRepositoryClient();
        _repositoryProjects = new Dictionary<string, List<string>>();
        var fakeNuGetPackagesSearcher = new FakeNuGetPackagesSearcher(_repositoryProjects);
        _nugetPackageUpdateOrderBuilder = new NugetPackageUpdateOrderBuilder(nugetRepositoryClient, fakeNuGetPackagesSearcher, TestLoggerProvider.GetLogger());
    }

    [Fact]
    public async Task Build_ForKysectRepositories_ReturnExpectedResult()
    {
        var expectedLinks = new List<GraphLink<string>>()
        {
            new("Kysect.Editorconfig", "Kysect.CommonLib"),
            new("Kysect.Editorconfig", "Kysect.GithubUtils"),
            new("Kysect.Editorconfig", "Kysect.DotnetProjectSystem"),
            new("Kysect.CommonLib", "Kysect.GithubUtils"),
            new("Kysect.CommonLib", "Kysect.DotnetProjectSystem"),
        };

        _repositoryProjects["Kysect.Editorconfig"] =
        [
            "Kysect.Editorconfig",
        ];

        _repositoryProjects["Kysect.CommonLib"] =
        [
            "Kysect.CommonLib",
            "Kysect.CommonLib.DependencyInjection"
        ];

        _repositoryProjects["Kysect.GithubUtils"] = ["Kysect.GithubUtils"];

        _repositoryProjects["Kysect.DotnetProjectSystem"] =
        [
            "Kysect.DotnetProjectSystem",
            "Kysect.DotnetProjectSystem.Tests"
        ];

        _repositoryProjects["Kysect.Zeya"] = ["Kysect.Zeya"];

        List<GraphLink<string>> dependencyLinks = await _nugetPackageUpdateOrderBuilder.Build(new List<ILocalRepository>()
        {
            _localRepositoryProvider.GetLocalRepository("Kysect.Editorconfig", "*"),
            _localRepositoryProvider.GetLocalRepository("Kysect.CommonLib", "*"),
            _localRepositoryProvider.GetLocalRepository("Kysect.GithubUtils", "*"),
            _localRepositoryProvider.GetLocalRepository("Kysect.DotnetProjectSystem", "*"),
            _localRepositoryProvider.GetLocalRepository("Kysect.Zeya", "*"),
        });

        dependencyLinks.Should().BeEquivalentTo(expectedLinks);
    }
}