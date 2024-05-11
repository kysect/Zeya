using FluentAssertions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryDependencies;
using Kysect.Zeya.RepositoryDependencies.PackageDataCollecting;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.RepositoryDependencies;

public class NugetPackageUpdateOrderBuilderTests
{
    private readonly NugetPackageUpdateOrderBuilder _nugetPackageUpdateOrderBuilder;
    private readonly LocalRepositoryProvider _localRepositoryProvider;

    public NugetPackageUpdateOrderBuilderTests()
    {
        var validationTestFixture = new ValidationTestFixture();
        _localRepositoryProvider = validationTestFixture.GetRequiredService<LocalRepositoryProvider>();

        _nugetPackageUpdateOrderBuilder = new NugetPackageUpdateOrderBuilder(validationTestFixture.GetLogger<NugetPackageUpdateOrderBuilder>());
    }

    [Fact]
    public void Build_ForKysectRepositories_ReturnExpectedResult()
    {
        var expectedLinks = new List<RepositoryDependencyLink>()
        {
            new("Kysect.Editorconfig", "Kysect.CommonLib", true),
            new("Kysect.Editorconfig", "Kysect.GithubUtils", true),
            new("Kysect.Editorconfig", "Kysect.DotnetProjectSystem", true),
            new("Kysect.CommonLib", "Kysect.GithubUtils", true),
            new("Kysect.CommonLib", "Kysect.DotnetProjectSystem", false),
        };

        ILocalRepository editorConfigRepository = _localRepositoryProvider.GetLocalRepository("Kysect.Editorconfig", "*.sln");
        ILocalRepository commonLibRepository = _localRepositoryProvider.GetLocalRepository("Kysect.CommonLib", "*.sln");
        ILocalRepository githubUtilsRepository = _localRepositoryProvider.GetLocalRepository("Kysect.GithubUtils", "*.sln");
        ILocalRepository dotnetProjectSystemRepository = _localRepositoryProvider.GetLocalRepository("Kysect.DotnetProjectSystem", "*.sln");
        var repositories = new List<ILocalRepository>()
        {
            editorConfigRepository,
            commonLibRepository,
            githubUtilsRepository,
            dotnetProjectSystemRepository,
        };

        IReadOnlyCollection<SolutionPackageAnalyzerContextItem> solutionPackageAnalyzerContextItems = new List<SolutionPackageAnalyzerContextItem>
        {
            new SolutionPackageAnalyzerContextItem(
                editorConfigRepository,
                [new ProjectPackageVersion("Kysect.Editorconfig", "1.0.0")],
                []),
            new SolutionPackageAnalyzerContextItem(
                commonLibRepository,
                [new ProjectPackageVersion("Kysect.CommonLib", "1.0.1")],
                [new ProjectPackageVersion("Kysect.Editorconfig", "1.0.0")]),
            new SolutionPackageAnalyzerContextItem(
                githubUtilsRepository,
                [new ProjectPackageVersion("Kysect.GithubUtils", "1.0.0-alpha1")],
                [new ProjectPackageVersion("Kysect.Editorconfig", "1.0.0"), new ProjectPackageVersion("Kysect.CommonLib", "1.0.1")]),
            new SolutionPackageAnalyzerContextItem(
                dotnetProjectSystemRepository,
                [new ProjectPackageVersion("Kysect.DotnetProjectSystem", "1.0.0")],
                [new ProjectPackageVersion("Kysect.Editorconfig", "1.0.0"), new ProjectPackageVersion("Kysect.CommonLib", "1.0.0")]),
        };

        IReadOnlyCollection<RepositoryDependencyLink> dependencyLinks = _nugetPackageUpdateOrderBuilder.CreateDependencyLinks(repositories, solutionPackageAnalyzerContextItems);

        dependencyLinks.Should().BeEquivalentTo(expectedLinks);
    }

    [Fact]
    public void CreateFixingActionPlan_SampleSolutionGroup_ReturnExpectedActionPlan()
    {
        ILocalRepository editorConfigRepository = _localRepositoryProvider.GetLocalRepository("Kysect.Editorconfig", "*.sln");
        ILocalRepository commonLibRepository = _localRepositoryProvider.GetLocalRepository("Kysect.CommonLib", "*.sln");
        ILocalRepository githubUtilsRepository = _localRepositoryProvider.GetLocalRepository("Kysect.GithubUtils", "*.sln");
        ILocalRepository dotnetProjectSystemRepository = _localRepositoryProvider.GetLocalRepository("Kysect.DotnetProjectSystem", "*.sln");
        var repositories = new List<ILocalRepository>()
        {
            editorConfigRepository,
            dotnetProjectSystemRepository,
            commonLibRepository,
            githubUtilsRepository,
        };

        var links = new List<RepositoryDependencyLink>()
        {
            new("Kysect.Editorconfig", "Kysect.DotnetProjectSystem", false),
            new("Kysect.Editorconfig", "Kysect.CommonLib", true),
            new("Kysect.CommonLib", "Kysect.GithubUtils", true),
        };

        List<string> repositoriesWithDiagnostics = ["Kysect.CommonLib"];


        var actionPlan = _nugetPackageUpdateOrderBuilder.CreateFixingActionPlan(repositories, repositoriesWithDiagnostics, links).ToList();
        actionPlan.Should().HaveCount(3);

        actionPlan[0].Repository.GetRepositoryName().Should().Be("Kysect.DotnetProjectSystem");
        actionPlan[0].FixRequired.Should().BeFalse();
        actionPlan[0].NotUpdatedInternalReferences.Should().BeEquivalentTo(["Kysect.Editorconfig"]);

        actionPlan[1].Repository.GetRepositoryName().Should().Be("Kysect.CommonLib");
        actionPlan[1].FixRequired.Should().BeTrue();
        actionPlan[1].NotUpdatedInternalReferences.Should().BeEquivalentTo([]);

        actionPlan[2].Repository.GetRepositoryName().Should().Be("Kysect.GithubUtils");
        actionPlan[2].FixRequired.Should().BeFalse();
        actionPlan[2].NotUpdatedInternalReferences.Should().BeEquivalentTo(["Kysect.CommonLib"]);
    }
}