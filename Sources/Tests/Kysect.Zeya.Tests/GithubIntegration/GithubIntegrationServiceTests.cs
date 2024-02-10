using FluentAssertions;
using Kysect.PowerShellRunner.Accessors;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.Tests.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System.IO.Abstractions;
using Kysect.PowerShellRunner.Abstractions.Accessors;

namespace Kysect.Zeya.Tests.GithubIntegration;

public class GithubIntegrationServiceTests : IDisposable
{
    private readonly TestTemporaryDirectory _temporaryDirectory;
    private readonly string _repositoriesDirectory;
    private readonly FileSystem _fileSystem;
    private readonly GithubRepositoryName _githubRepositoryName;
    private readonly GithubIntegrationService _githubIntegrationService;
    private readonly IPowerShellAccessor _powerShellAccessor;
    private readonly ILogger _logger;

    public GithubIntegrationServiceTests()
    {
        _logger = TestLoggerProvider.GetLogger();
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

        _powerShellAccessor = new PowerShellAccessorDecoratorBuilder(new PowerShellAccessorFactory())
            .WithLogging(_logger)
            .Build();

        var localStoragePathFactory = new FakePathFormatStrategy(_repositoriesDirectory);
        _githubIntegrationService = new GithubIntegrationService(githubIntegrationOptions, new GitHubClient(new ProductHeaderValue("Zeya")), localStoragePathFactory, _powerShellAccessor, _logger);
        _githubRepositoryName = new GithubRepositoryName("Kysect", "Zeya");
    }

    [Fact]
    public void CloneOrUpdate_NewRepository_GitDirectoryCreated()
    {
        var gitDirectory = _fileSystem.Path.Combine(_repositoriesDirectory, ".git");

        _logger.LogInformation("Clone repository to directory {Directory}", _repositoriesDirectory);
        _githubIntegrationService.CloneOrUpdate(_githubRepositoryName);

        _fileSystem.Directory.Exists(gitDirectory).Should().BeTrue();
    }

    [Fact]
    public void CloneOrUpdate_SecondCall_DoNotThrowException()
    {
        var gitDirectory = _fileSystem.Path.Combine(_repositoriesDirectory, ".git");

        _logger.LogInformation("Clone repository to directory {Directory}", _repositoriesDirectory);
        _githubIntegrationService.CloneOrUpdate(_githubRepositoryName);
        _logger.LogInformation("Clone repository to directory {Directory}", _repositoriesDirectory);
        _githubIntegrationService.CloneOrUpdate(_githubRepositoryName);

        _fileSystem.Directory.Exists(gitDirectory).Should().BeTrue();
    }

    public void Dispose()
    {
        _powerShellAccessor.Dispose();
        _temporaryDirectory.Dispose();
    }
}