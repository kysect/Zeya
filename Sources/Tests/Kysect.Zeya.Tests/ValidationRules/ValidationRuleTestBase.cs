using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Asserts;
using Kysect.Zeya.Tests.Tools;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRules;

public abstract class ValidationRuleTestBase
{
    protected ILogger Logger { get; }
    protected ClonedGithubRepositoryAccessor Repository { get; }
    protected GithubRepositoryAccessorFactory GithubRepositoryAccessorFactory { get; }
    protected MockFileSystem FileSystem { get; }
    protected ScenarioContext Context { get; }
    protected XmlDocumentSyntaxFormatter Formatter { get; }
    protected RepositoryDiagnosticCollectorAsserts DiagnosticCollectorAsserts { get; }
    protected string CurrentPath { get; }
    protected SolutionFileContentParser SolutionFileContentParser { get; }
    protected RepositorySolutionAccessorFactory RepositorySolutionAccessorFactory { get; }
    protected FileSystemAsserts FileSystemAsserts { get; }

    protected ValidationRuleTestBase()
    {
        Logger = TestLoggerProvider.GetLogger();
        Formatter = new XmlDocumentSyntaxFormatter();
        FileSystem = new MockFileSystem();
        CurrentPath = FileSystem.Path.GetFullPath(".");

        DiagnosticCollectorAsserts = new RepositoryDiagnosticCollectorAsserts("MockRepository");
        GithubRepositoryAccessorFactory = new GithubRepositoryAccessorFactory(new FakePathFormatStrategy(CurrentPath), FileSystem);
        Repository = GithubRepositoryAccessorFactory.Create(new GithubRepository("owner", "name"));

        RepositoryDiagnosticCollector diagnosticCollector = DiagnosticCollectorAsserts.GetCollector();
        Context = RepositoryValidationContextExtensions.CreateScenarioContext(new RepositoryValidationContext(Repository, diagnosticCollector));

        SolutionFileContentParser = new SolutionFileContentParser();
        RepositorySolutionAccessorFactory = new RepositorySolutionAccessorFactory(SolutionFileContentParser, FileSystem);

        FileSystemAsserts = new FileSystemAsserts(FileSystem);
    }
}