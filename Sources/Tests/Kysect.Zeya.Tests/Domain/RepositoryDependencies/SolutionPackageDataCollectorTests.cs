using FluentAssertions;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryDependencies.PackageDataCollecting;
using Kysect.Zeya.RepositoryDependencies;
using Kysect.Zeya.Tests.Tools;
using NuGet.Versioning;

namespace Kysect.Zeya.Tests.Domain.RepositoryDependencies;

public class FakePackageRepositoryClient(Dictionary<string, NuGetVersion> versions) : IPackageRepositoryClient
{
    public Task<NuGetVersion?> TryGetLastVersion(string nugetName)
    {
        if (versions.TryGetValue(nugetName, out NuGetVersion? value))
            return Task.FromResult<NuGetVersion?>(value);

        return Task.FromResult<NuGetVersion?>(null);
    }
}

public class SolutionPackageDataCollectorTests
{
    private readonly LocalRepositoryProvider _localRepositoryProvider;
    private readonly SolutionFileStructureBuilderFactory _solutionFileStructureBuilderFactory;
    private readonly Dictionary<string, NuGetVersion> _packageVersions;
    private readonly SolutionPackageDataCollector _solutionPackageDataCollector;

    public SolutionPackageDataCollectorTests()
    {
        var validationTestFixture = new ValidationTestFixture();
        _localRepositoryProvider = validationTestFixture.GetRequiredService<LocalRepositoryProvider>();

        var solutionFileContentParser = new SolutionFileContentParser();
        _packageVersions = new Dictionary<string, NuGetVersion>();
        var fakePackageRepositoryClient = new FakePackageRepositoryClient(_packageVersions);
        _solutionFileStructureBuilderFactory = validationTestFixture.GetRequiredService<SolutionFileStructureBuilderFactory>();
        _solutionPackageDataCollector = new SolutionPackageDataCollector(solutionFileContentParser, fakePackageRepositoryClient, validationTestFixture.GetLogger<SolutionPackageDataCollector>());
    }

    [Fact]
    public async Task Build_ForKysectRepositories_ReturnExpectedResult()
    {
        var expectedLinks = new List<RepositoryDependencyLink>()
        {
            new("Kysect.Editorconfig", "Kysect.CommonLib", true),
            new("Kysect.Editorconfig", "Kysect.GithubUtils", true),
            new("Kysect.Editorconfig", "Kysect.DotnetProjectSystem", true),
            new("Kysect.CommonLib", "Kysect.GithubUtils", true),
            new("Kysect.CommonLib", "Kysect.DotnetProjectSystem", false),
        };

        _packageVersions["Kysect.Editorconfig"] = new NuGetVersion("1.0.0");
        _packageVersions["Kysect.CommonLib"] = new NuGetVersion("1.0.1");
        _packageVersions["Kysect.DotnetProjectSystem"] = new NuGetVersion("1.0.0");
        _packageVersions["Kysect.GithubUtils"] = new NuGetVersion("1.0.0-alpha1");

        ILocalRepository editorConfigRepository = _localRepositoryProvider.GetLocalRepository("Kysect.Editorconfig", "*.sln");
        var editorConfigRepositoryPackages = DirectoryPackagesPropsFile.CreateEmpty();
        _solutionFileStructureBuilderFactory.Create("Kysect.Editorconfig")
                .AddProject(new ProjectFileStructureBuilder("Kysect.Editorconfig"))
                .AddDirectoryPackagesProps(editorConfigRepositoryPackages)
                .Save(editorConfigRepository.FileSystem.GetFullPath());

        ILocalRepository commonLibRepository = _localRepositoryProvider.GetLocalRepository("Kysect.CommonLib", "*.sln");
        var commonLibRepositoryPackages = DirectoryPackagesPropsFile.CreateEmpty();
        commonLibRepositoryPackages.Versions.AddPackageVersion("Kysect.Editorconfig", "1.0.0");
        _solutionFileStructureBuilderFactory.Create("Kysect.CommonLib")
            .AddProject(new ProjectFileStructureBuilder("Kysect.CommonLib"))
            .AddProject(new ProjectFileStructureBuilder("Kysect.CommonLib.DependencyInjection"))
            .AddDirectoryPackagesProps(commonLibRepositoryPackages)
            .Save(commonLibRepository.FileSystem.GetFullPath());

        ILocalRepository githubUtilsRepository = _localRepositoryProvider.GetLocalRepository("Kysect.GithubUtils", "*.sln");
        var githubUtilsRepositoryPackages = DirectoryPackagesPropsFile.CreateEmpty();
        githubUtilsRepositoryPackages.Versions.AddPackageVersion("Kysect.Editorconfig", "1.0.0");
        githubUtilsRepositoryPackages.Versions.AddPackageVersion("Kysect.CommonLib", "1.0.1");
        _solutionFileStructureBuilderFactory.Create("Kysect.GithubUtils")
            .AddProject(new ProjectFileStructureBuilder("Kysect.GithubUtils"))
            .AddDirectoryPackagesProps(githubUtilsRepositoryPackages)
            .Save(githubUtilsRepository.FileSystem.GetFullPath());

        ILocalRepository dotnetProjectSystemRepository = _localRepositoryProvider.GetLocalRepository("Kysect.DotnetProjectSystem", "*.sln");
        var dotnetProjectSystemRepositoryPackages = DirectoryPackagesPropsFile.CreateEmpty();
        dotnetProjectSystemRepositoryPackages.Versions.AddPackageVersion("Kysect.Editorconfig", "1.0.0");
        dotnetProjectSystemRepositoryPackages.Versions.AddPackageVersion("Kysect.CommonLib", "1.0.0");
        _solutionFileStructureBuilderFactory.Create("Kysect.DotnetProjectSystem")
            .AddProject(new ProjectFileStructureBuilder("Kysect.DotnetProjectSystem"))
            .AddProject(new ProjectFileStructureBuilder("Kysect.DotnetProjectSystem.Tests"))
            .AddDirectoryPackagesProps(dotnetProjectSystemRepositoryPackages)
            .Save(dotnetProjectSystemRepository.FileSystem.GetFullPath());


        var repositories = new List<ILocalRepository>()
        {
            editorConfigRepository,
            commonLibRepository,
            githubUtilsRepository,
            dotnetProjectSystemRepository,
        };

        List<SolutionPackageAnalyzerContextItem> solutionPackageAnalyzerContextItems = (await _solutionPackageDataCollector.Collect(repositories)).ToList();
        solutionPackageAnalyzerContextItems.Should().HaveCount(4);

        solutionPackageAnalyzerContextItems[0].Repository.GetRepositoryName().Should().Be("Kysect.Editorconfig");
        solutionPackageAnalyzerContextItems[0].DeclaredPackages.Should().BeEquivalentTo([new ProjectPackageVersion("Kysect.Editorconfig", "1.0.0")]);
        solutionPackageAnalyzerContextItems[0].DependencyPackages.Should().BeEmpty();

        solutionPackageAnalyzerContextItems[1].Repository.GetRepositoryName().Should().Be("Kysect.CommonLib");
        solutionPackageAnalyzerContextItems[1].DeclaredPackages.Should().BeEquivalentTo([new ProjectPackageVersion("Kysect.CommonLib", "1.0.1")]);
        solutionPackageAnalyzerContextItems[1].DependencyPackages.Should().BeEquivalentTo([new ProjectPackageVersion("Kysect.Editorconfig", "1.0.0")]);

        solutionPackageAnalyzerContextItems[2].Repository.GetRepositoryName().Should().Be("Kysect.GithubUtils");
        solutionPackageAnalyzerContextItems[2].DeclaredPackages.Should().BeEquivalentTo([new ProjectPackageVersion("Kysect.GithubUtils", "1.0.0-alpha1")]);
        solutionPackageAnalyzerContextItems[2].DependencyPackages.Should().BeEquivalentTo([new ProjectPackageVersion("Kysect.Editorconfig", "1.0.0"), new ProjectPackageVersion("Kysect.CommonLib", "1.0.1")]);

        solutionPackageAnalyzerContextItems[3].Repository.GetRepositoryName().Should().Be("Kysect.DotnetProjectSystem");
        solutionPackageAnalyzerContextItems[3].DeclaredPackages.Should().BeEquivalentTo([new ProjectPackageVersion("Kysect.DotnetProjectSystem", "1.0.0")]);
        solutionPackageAnalyzerContextItems[3].DependencyPackages.Should().BeEquivalentTo([new ProjectPackageVersion("Kysect.Editorconfig", "1.0.0"), new ProjectPackageVersion("Kysect.CommonLib", "1.0.0")]);
    }
}