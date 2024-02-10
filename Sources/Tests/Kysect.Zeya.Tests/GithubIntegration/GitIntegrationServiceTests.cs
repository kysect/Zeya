using FluentAssertions;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.Tests.Fakes;
using Kysect.Zeya.Tests.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using Repository = LibGit2Sharp.Repository;

namespace Kysect.Zeya.Tests.GithubIntegration;

public class GitIntegrationServiceTests : IDisposable
{
    private readonly TestTemporaryDirectory _temporaryDirectory;
    private readonly GitIntegrationService _gitIntegrationService;
    private readonly string _repositoriesDirectory;
    private readonly FileSystem _fileSystem;
    private readonly GithubRepository _githubRepository;
    private readonly ClonedRepositoryAccessor _clonedRepositoryAccessor;
    private readonly FakeGithubIntegrationService _githubIntegrationService;

    public GitIntegrationServiceTests()
    {
        ILogger logger = TestLoggerProvider.GetLogger();
        _fileSystem = new FileSystem();
        string testDirectory = "Test-directory";
        _repositoriesDirectory = _fileSystem.Path.Combine(".", testDirectory, "Repositories");

        _temporaryDirectory = new TestTemporaryDirectory(_fileSystem, testDirectory);
        var githubIntegrationOptions = new OptionsWrapper<GithubIntegrationOptions>(new GithubIntegrationOptions()
        {
            CommitAuthor = new GitCommitAuthor()
            {
                GithubUsername = "Name",
                GithubMail = "Name@null.com",
            }
        });
        var localStoragePathFactory = new FakePathFormatStrategy(_repositoriesDirectory);
        _gitIntegrationService = new GitIntegrationService(commitAuthor: null);
        _githubIntegrationService = new FakeGithubIntegrationService(githubIntegrationOptions, localStoragePathFactory, logger);
        _githubRepository = new GithubRepository("Kysect", "Zeya");
        _clonedRepositoryAccessor = new ClonedRepositoryAccessor(_repositoriesDirectory, _fileSystem);
    }

    [Fact]
    public void CloneOrUpdate_NewRepository_GitDirectoryCreated()
    {
        var gitDirectory = _fileSystem.Path.Combine(_repositoriesDirectory, ".git");

        _githubIntegrationService.CloneOrUpdate(_githubRepository);

        _fileSystem.Directory.Exists(gitDirectory).Should().BeTrue();
    }

    [Fact]
    public void CloneOrUpdate_SecondCall_DoNotThrowException()
    {
        var gitDirectory = _fileSystem.Path.Combine(_repositoriesDirectory, ".git");

        _githubIntegrationService.CloneOrUpdate(_githubRepository);
        _githubIntegrationService.CloneOrUpdate(_githubRepository);

        _fileSystem.Directory.Exists(gitDirectory).Should().BeTrue();
    }

    [Fact]
    public void CreateFixBranch_OnMasterBranch_ChangeToNewBranch()
    {
        _githubIntegrationService.CloneOrUpdate(_githubRepository);
        using var gitRepository = new Repository(_repositoriesDirectory);

        gitRepository.Head.FriendlyName.Should().Be("master");
        _gitIntegrationService.CreateFixBranch(_clonedRepositoryAccessor, "new-branch");

        gitRepository.Head.FriendlyName.Should().Be("new-branch");
    }

    [Fact]
    public void CreateCommitWithFix_OnMasterBranch_ChangeLastCommit()
    {
        _githubIntegrationService.CloneOrUpdate(_githubRepository);
        using var gitRepository = new Repository(_repositoriesDirectory);

        _fileSystem.File.Create(_fileSystem.Path.Combine(_repositoriesDirectory, "file.txt")).Dispose();
        _gitIntegrationService.CreateCommitWithFix(_clonedRepositoryAccessor, "Commit message");

        gitRepository.Head.Commits.First().Message.Trim().Should().Be("Commit message");
    }

    public void Dispose()
    {
        _temporaryDirectory.Dispose();
    }
}