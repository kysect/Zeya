using FluentAssertions;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.Tests.Tools;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using Repository = LibGit2Sharp.Repository;

namespace Kysect.Zeya.Tests.Integration.GitIntegration;

public class GitIntegrationServiceTests : IDisposable
{
    private readonly TestTemporaryDirectory _temporaryDirectory;
    private readonly IGitIntegrationService _gitIntegrationService;
    private readonly string _repositoriesDirectory;
    private readonly FileSystem _fileSystem;
    private readonly IRepositoryFetcher _repositoryFetcher;
    private readonly GithubRepository _githubRepositoryName;

    public GitIntegrationServiceTests()
    {
        _fileSystem = new FileSystem();
        _temporaryDirectory = new TestTemporaryDirectory(_fileSystem);
        _repositoriesDirectory = _temporaryDirectory.GetTemporaryDirectory();

        var validationTestFixture = new ValidationTestFixture();
        _gitIntegrationService = new GitIntegrationService(
            validationTestFixture.GetRequiredService<IOptions<GitEnvironmentOptions>>().Value.CommitAuthor,
            validationTestFixture.GetRequiredService<IRepositoryFetcher>(),
            validationTestFixture.GetRequiredService<GitRepositoryCredentialOptions>());
        _repositoryFetcher = validationTestFixture.GetRequiredService<IRepositoryFetcher>();
        _githubRepositoryName = new GithubRepository("Kysect", "Zeya");
    }

    [Fact]
    public void CloneOrUpdate_NewRepository_GitDirectoryCreated()
    {
        var gitDirectory = _fileSystem.Path.Combine(_repositoriesDirectory, ".git");

        _repositoryFetcher.EnsureRepositoryUpdated(_repositoriesDirectory, _githubRepositoryName);

        _fileSystem.Directory.Exists(gitDirectory).Should().BeTrue();
    }

    [Fact]
    public void CloneOrUpdate_SecondCall_DoNotThrowException()
    {
        var gitDirectory = _fileSystem.Path.Combine(_repositoriesDirectory, ".git");

        _repositoryFetcher.EnsureRepositoryUpdated(_repositoriesDirectory, _githubRepositoryName);
        _repositoryFetcher.EnsureRepositoryUpdated(_repositoriesDirectory, _githubRepositoryName);

        _fileSystem.Directory.Exists(gitDirectory).Should().BeTrue();
    }

    [Fact]
    public void CreateFixBranch_OnMasterBranch_ChangeToNewBranch()
    {
        _repositoryFetcher.EnsureRepositoryUpdated(_repositoriesDirectory, _githubRepositoryName);
        using var gitRepository = new Repository(_repositoriesDirectory);

        gitRepository.Head.FriendlyName.Should().Be("master");
        _gitIntegrationService.CreateFixBranch(_repositoriesDirectory, "new-branch");

        gitRepository.Head.FriendlyName.Should().Be("new-branch");
    }

    [Fact]
    public void CreateCommitWithFix_OnMasterBranch_ChangeLastCommit()
    {
        _repositoryFetcher.EnsureRepositoryUpdated(_repositoriesDirectory, _githubRepositoryName);
        using var gitRepository = new Repository(_repositoriesDirectory);

        _fileSystem.File.Create(_fileSystem.Path.Combine(_repositoriesDirectory, "file.txt")).Dispose();
        _gitIntegrationService.CreateCommitWithFix(_repositoriesDirectory, "Commit message");

        gitRepository.Head.Commits.First().Message.Trim().Should().Be("Commit message");
    }

    [Fact]
    public void GetDiff_AfterChanges_ReturnChanges()
    {
        string filePath = _fileSystem.Path.Combine(_repositoriesDirectory, "file.txt");
        var expected = """
                       diff --git a/file.txt b/file.txt
                       new file mode 100644
                       index 0000000..3b2e0ae
                       --- /dev/null
                       +++ b/file.txt
                       @@ -0,0 +1 @@
                       +Some changes qer
                       \ No newline at end of file

                       """;

        _repositoryFetcher.EnsureRepositoryUpdated(_repositoriesDirectory, _githubRepositoryName);
        using var gitRepository = new Repository(_repositoriesDirectory);

        _fileSystem.File.WriteAllText(filePath, "Some changes qer");
        string diff = _gitIntegrationService.GetDiff(_repositoriesDirectory);

        // KB: git return unix end lines
        expected = expected.NormalizeEndLines();
        diff = diff.NormalizeEndLines();

        diff.Should().Be(expected);
    }

    public void Dispose()
    {
        _temporaryDirectory.Dispose();
    }
}