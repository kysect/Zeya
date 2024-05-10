using FluentAssertions;
using Kysect.CommonLib.Graphs;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryDependencies;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Domain.RepositoryDependencies;

public class NugetPackageUpdateOrderBuilderTests
{
    private readonly NugetPackageUpdateOrderBuilder _nugetPackageUpdateOrderBuilder;
    private readonly LocalRepositoryProvider _localRepositoryProvider;
    private readonly SolutionFileStructureBuilderFactory _solutionFileStructureBuilderFactory;

    public NugetPackageUpdateOrderBuilderTests()
    {
        var validationTestFixture = new ValidationTestFixture();
        _localRepositoryProvider = validationTestFixture.GetRequiredService<LocalRepositoryProvider>();

        var solutionFileContentParser = new SolutionFileContentParser();
        _nugetPackageUpdateOrderBuilder = new NugetPackageUpdateOrderBuilder(solutionFileContentParser, validationTestFixture.GetLogger<NugetPackageUpdateOrderBuilder>());
        _solutionFileStructureBuilderFactory = validationTestFixture.GetRequiredService<SolutionFileStructureBuilderFactory>();
    }

    [Fact]
    public void Build_ForKysectRepositories_ReturnExpectedResult()
    {
        var expectedLinks = new List<GraphLink<string>>()
        {
            new("Kysect.Editorconfig", "Kysect.CommonLib"),
            new("Kysect.Editorconfig", "Kysect.GithubUtils"),
            new("Kysect.Editorconfig", "Kysect.DotnetProjectSystem"),
            new("Kysect.CommonLib", "Kysect.GithubUtils"),
            new("Kysect.CommonLib", "Kysect.DotnetProjectSystem"),
        };

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
        githubUtilsRepositoryPackages.Versions.AddPackageVersion("Kysect.CommonLib", "1.0.0");
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

        IReadOnlyCollection<GraphLink<string>> dependencyLinks = _nugetPackageUpdateOrderBuilder.Build(new List<ILocalRepository>()
        {
            editorConfigRepository,
            commonLibRepository,
            githubUtilsRepository,
            dotnetProjectSystemRepository,
        });

        dependencyLinks.Should().BeEquivalentTo(expectedLinks);
    }
}