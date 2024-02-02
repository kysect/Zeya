using FluentAssertions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.Tests.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using Repository = LibGit2Sharp.Repository;

namespace Kysect.Zeya.Tests.GithubIntegration;

public class GitIntegrationServiceTests : IDisposable
{
    private readonly TestTemporaryDirectory _temporaryDirectory;
    private readonly GitIntegrationService _githubIntegrationService;
    private readonly string _repositoriesDirectory;
    private readonly FileSystem _fileSystem;
    private readonly GithubRepository _githubRepository;

    public GitIntegrationServiceTests()
    {
        ILogger logger = TestLoggerProvider.GetLogger();
        _fileSystem = new FileSystem();
        string testDirectory = "Test-directory";
        _repositoriesDirectory = _fileSystem.Path.Combine(".", testDirectory, "Repositories");

        _temporaryDirectory = new TestTemporaryDirectory(_fileSystem, testDirectory);
        var githubIntegrationOptions = new OptionsWrapper<GithubIntegrationOptions>(new GithubIntegrationOptions());
        var localStoragePathFactory = new FakePathFormatStrategy(_repositoriesDirectory);
        _githubIntegrationService = new GitIntegrationService(githubIntegrationOptions, localStoragePathFactory, logger);
        _githubRepository = new GithubRepository("Kysect", "Zeya");
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
        _githubIntegrationService.CreateFixBranch(_githubRepository, "new-branch");

        gitRepository.Head.FriendlyName.Should().Be("new-branch");
    }

    [Fact]
    public void CreateCommitWithFix_OnMasterBranch_ChangeLastCommit()
    {
        _githubIntegrationService.CloneOrUpdate(_githubRepository);
        using var gitRepository = new Repository(_repositoriesDirectory);

        _fileSystem.File.Create(_fileSystem.Path.Combine(_repositoriesDirectory, "file.txt")).Dispose();
        _githubIntegrationService.CreateCommitWithFix(_githubRepository, "Commit message");

        gitRepository.Head.Commits.First().Message.Trim().Should().Be("Commit message");
    }

    public void Dispose()
    {
        _temporaryDirectory.Dispose();
    }
}