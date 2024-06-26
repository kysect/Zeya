﻿using FluentAssertions;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.Tests.Tools;
using Microsoft.Extensions.Logging;
using Octokit;
using System.IO.Abstractions;
using Kysect.Zeya.Tests.Tools.Fakes;
using Kysect.Zeya.GithubIntegration.Abstraction;
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

        var localStoragePathFactory = new FakePathFormatStrategy(_repositoriesDirectory);
        _githubIntegrationService = new GithubIntegrationService(
            new GitHubClient(new ProductHeaderValue("Zeya")),
            serviceProvider.GetRequiredService<ILogger<GithubIntegrationService>>());

        _githubRepositoryName = new GithubRepositoryName("Kysect", "Zeya");
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