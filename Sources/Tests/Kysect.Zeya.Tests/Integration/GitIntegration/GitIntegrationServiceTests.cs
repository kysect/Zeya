using FluentAssertions;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using Repository = LibGit2Sharp.Repository;

namespace Kysect.Zeya.Tests.Integration.GitIntegration;

public class GitIntegrationServiceTests : IDisposable
{
    private readonly TestTemporaryDirectory _temporaryDirectory;
    private readonly IGitIntegrationService _gitIntegrationService;
    private readonly string _repositoriesDirectory;
    private readonly FileSystem _fileSystem;
    private readonly GithubRepositoryName _githubRepositoryName;
    private readonly LocalRepository _localRepository;
    private readonly FakeGithubIntegrationService _githubIntegrationService;

    public GitIntegrationServiceTests()
    {
        ILogger logger = TestLoggerProvider.GetLogger();
        _fileSystem = new FileSystem();
        _temporaryDirectory = new TestTemporaryDirectory(_fileSystem);
        _repositoriesDirectory = _temporaryDirectory.GetTemporaryDirectory();

        var githubIntegrationOptions = new GithubIntegrationOptions()
        {
            CommitAuthor = new GitCommitAuthor()
            {
                GithubUsername = "Name",
                GithubMail = "Name@null.com",
            },
            Credential = new GithubIntegrationCredential()
            {
                GithubToken = "token",
                GithubUsername = "Name",
            }
        };
        var localStoragePathFactory = new FakePathFormatStrategy(_repositoriesDirectory);
        _gitIntegrationService = new GitIntegrationService(githubIntegrationOptions.CommitAuthor);
        _githubIntegrationService = new FakeGithubIntegrationService(githubIntegrationOptions.Credential, localStoragePathFactory, logger);
        _githubRepositoryName = new GithubRepositoryName("Kysect", "Zeya");
        var dotnetSolutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, new SolutionFileContentParser(), new XmlDocumentSyntaxFormatter());
        _localRepository = new LocalRepository(_repositoriesDirectory, LocalRepositorySolutionManager.DefaultMask, _fileSystem, dotnetSolutionModifierFactory);
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
        _gitIntegrationService.CreateFixBranch(_localRepository.FileSystem.GetFullPath(), "new-branch");

        gitRepository.Head.FriendlyName.Should().Be("new-branch");
    }

    [Fact]
    public void CreateCommitWithFix_OnMasterBranch_ChangeLastCommit()
    {
        _githubIntegrationService.CloneOrUpdate(_githubRepositoryName);
        using var gitRepository = new Repository(_repositoriesDirectory);

        _fileSystem.File.Create(_fileSystem.Path.Combine(_repositoriesDirectory, "file.txt")).Dispose();
        _gitIntegrationService.CreateCommitWithFix(_localRepository.FileSystem.GetFullPath(), "Commit message");

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

        _githubIntegrationService.CloneOrUpdate(_githubRepositoryName);
        using var gitRepository = new Repository(_repositoriesDirectory);

        _fileSystem.File.WriteAllText(filePath, "Some changes qer");
        string diff = _gitIntegrationService.GetDiff(_localRepository.FileSystem.GetFullPath());

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