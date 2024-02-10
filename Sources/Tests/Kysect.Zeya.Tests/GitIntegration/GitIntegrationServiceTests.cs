using FluentAssertions;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using Repository = LibGit2Sharp.Repository;

namespace Kysect.Zeya.Tests.GitIntegration;

public class GitIntegrationServiceTests : IDisposable
{
    private readonly TestTemporaryDirectory _temporaryDirectory;
    private readonly GitIntegrationService _gitIntegrationService;
    private readonly string _repositoriesDirectory;
    private readonly FileSystem _fileSystem;
    private readonly GithubRepositoryName _githubRepositoryName;
    private readonly ClonedRepository _clonedRepository;
    private readonly FakeGithubIntegrationService _githubIntegrationService;

    public GitIntegrationServiceTests()
    {
        ILogger logger = TestLoggerProvider.GetLogger();
        _fileSystem = new FileSystem();
        _temporaryDirectory = new TestTemporaryDirectory(_fileSystem);
        _repositoriesDirectory = _temporaryDirectory.GetTemporaryDirectory();

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
        _githubRepositoryName = new GithubRepositoryName("Kysect", "Zeya");
        _clonedRepository = new ClonedRepository(_repositoriesDirectory, _fileSystem);
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
    public void CreateFixBranch_OnMasterBranch_ChangeToNewBranch()
    {
        _githubIntegrationService.CloneOrUpdate(_githubRepositoryName);
        using var gitRepository = new Repository(_repositoriesDirectory);

        gitRepository.Head.FriendlyName.Should().Be("master");
        _gitIntegrationService.CreateFixBranch(_clonedRepository, "new-branch");

        gitRepository.Head.FriendlyName.Should().Be("new-branch");
    }

    [Fact]
    public void CreateCommitWithFix_OnMasterBranch_ChangeLastCommit()
    {
        _githubIntegrationService.CloneOrUpdate(_githubRepositoryName);
        using var gitRepository = new Repository(_repositoriesDirectory);

        _fileSystem.File.Create(_fileSystem.Path.Combine(_repositoriesDirectory, "file.txt")).Dispose();
        _gitIntegrationService.CreateCommitWithFix(_clonedRepository, "Commit message");

        gitRepository.Head.Commits.First().Message.Trim().Should().Be("Commit message");
    }

    public void Dispose()
    {
        _temporaryDirectory.Dispose();
    }
}