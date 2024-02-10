using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.RepositoryValidation.Abstractions;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Asserts;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.ValidationRules;

public abstract class ValidationRuleTestBase
{
    protected ILogger Logger { get; }
    protected ClonedGithubRepository Repository { get; }
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
        Repository = new ClonedGithubRepository(new GithubRepositoryName("owner", "name"), CurrentPath, FileSystem);

        RepositoryDiagnosticCollector diagnosticCollector = DiagnosticCollectorAsserts.GetCollector();
        Context = RepositoryValidationContextExtensions.CreateScenarioContext(new RepositoryValidationContext(Repository, diagnosticCollector));

        SolutionFileContentParser = new SolutionFileContentParser();
        RepositorySolutionAccessorFactory = new RepositorySolutionAccessorFactory(SolutionFileContentParser, FileSystem);

        FileSystemAsserts = new FileSystemAsserts(FileSystem);
    }
}