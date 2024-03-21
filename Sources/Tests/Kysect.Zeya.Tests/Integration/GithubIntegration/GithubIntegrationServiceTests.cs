using FluentAssertions;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.Tests.Tools;
using Microsoft.Extensions.Logging;
using Octokit;
using System.IO.Abstractions;
using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.Tests.Tools.Fakes;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.Tests.Integration.GithubIntegration;

public class GithubIntegrationServiceTests : IDisposable
{
    private readonly TestTemporaryDirectory _temporaryDirectory;
    private readonly string _repositoriesDirectory;
    private readonly FileSystem _fileSystem;
    private readonly GithubRepositoryName _githubRepositoryName;
    private readonly GithubIntegrationService _githubIntegrationService;

    public GithubIntegrationServiceTests()
    {
        TestLoggerProvider.GetLogger();
        _fileSystem = new FileSystem();
        _temporaryDirectory = new TestTemporaryDirectory(_fileSystem);
        _repositoriesDirectory = _temporaryDirectory.GetTemporaryDirectory();

        IServiceProvider serviceProvider = new ServiceCollection()
            .AddZeyaTestLogging()
            .BuildServiceProvider();


        var githubIntegrationOptions = new GithubIntegrationOptions()
        {
            CommitAuthor = new GitCommitAuthor()
            {
                GithubUsername = "Name",
                GithubMail = "Name@null.com",
            },
            Credential = new GithubIntegrationCredential()
            {
                GithubUsername = "Name",
                GithubToken = "token"
            }
        };

        var localStoragePathFactory = new FakePathFormatStrategy(_repositoriesDirectory);
        _githubIntegrationService = new GithubIntegrationService(
            githubIntegrationOptions.Credential,
            new GitHubClient(new ProductHeaderValue("Zeya")),
            localStoragePathFactory,
            serviceProvider.GetRequiredService<ILogger<GithubIntegrationService>>());
        _githubRepositoryName = new GithubRepositoryName("Kysect", "Zeya");
    }

    [Fact(Skip = "Github has limit for requests")]
    public async Task GetOrganizationRepositories_ForKysect_ReturnZeya()
    {
        IReadOnlyCollection<GithubRepositoryName> repositories = await _githubIntegrationService.GetOrganizationRepositories("Kysect");

        repositories.Should().Contain(new GithubRepositoryName("Kysect", "Zeya"));
    }

    [Fact]
    public void CloneOrUpdate_NewRepository_GitDirectoryCreated()
    {
        var gitDirectory = _fileSystem.Path.Combine(_repositoriesDirectory, ".git");

        _githubIntegrationService.CloneOrUpdate(_githubRepositoryName);

        _fileSystem.Directory.Exists(gitDirectory).Should().BeTrue();
    }

    [Fact]
    public void CloneOrUpdate_SecondCall_DoNotThrowException()
    {
        var gitDirectory = _fileSystem.Path.Combine(_repositoriesDirectory, ".git");

        _githubIntegrationService.CloneOrUpdate(_githubRepositoryName);
        _githubIntegrationService.CloneOrUpdate(_githubRepositoryName);

        _fileSystem.Directory.Exists(gitDirectory).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteBranchOnMerge_ReturnExpectedResult()
    {
        bool deleteBranchOnMerge = await _githubIntegrationService.DeleteBranchOnMerge(_githubRepositoryName);

        deleteBranchOnMerge.Should().BeFalse();
    }

    [Fact(Skip = "Token is required")]
    public void GetRepositoryBranchProtection_ReturnExpectedResult()
    {
        var expected = new RepositoryBranchProtection(false, false);

        RepositoryBranchProtection repositoryBranchProtection = _githubIntegrationService.GetRepositoryBranchProtection(_githubRepositoryName, "master");

        repositoryBranchProtection.Should().Be(expected);
    }

    public void Dispose()
    {
        _temporaryDirectory.Dispose();
    }
}