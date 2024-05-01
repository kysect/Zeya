using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using Kysect.Zeya.RepositoryValidationRules.Rules;
using Kysect.Zeya.Tests.Tools.Asserts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Tools;

public class ValidationTestFixture
{
    private readonly IServiceProvider _serviceProvider;

    public XmlDocumentSyntaxFormatter Formatter { get; }
    public MockFileSystem FileSystem { get; }
    public string CurrentPath { get; }
    public LocalRepositoryProvider RepositoryProvider { get; }
    public SolutionFileStructureBuilderFactory SolutionFileStructureBuilderFactory { get; }

    public RepositoryDiagnosticCollectorAsserts DiagnosticCollectorAsserts { get; }
    public FileSystemAsserts FileSystemAsserts { get; }


    public ValidationTestFixture()
    {
        FileSystem = new MockFileSystem();
        CurrentPath = FileSystem.Path.GetFullPath(".");
        DiagnosticCollectorAsserts = new RepositoryDiagnosticCollectorAsserts();
        FileSystemAsserts = new FileSystemAsserts(FileSystem);

        _serviceProvider = new ServiceCollection()
            .AddZeyaTestLogging()
            .AddZeyaSqliteDbContext(Guid.NewGuid().ToString())
            //.AddSingleton(ZeyaDbContextTestProvider.CreateContext())
            .AddSingleton<SolutionFileStructureBuilderFactory>()

            .AddZeyaTestGitConfiguration()
            .AddZeyaTestGitIntegration()
            .AddZeyaTestGithubIntegration(CurrentPath)

            .AddSingleton<IFileSystem>(FileSystem)
            .AddZeyaDotnetProjectSystemIntegration()
            .AddZeyaValidationRulesAndFixers(typeof(RuleDescription).Assembly)
            .AddZeyaRepositoryValidation()
            .AddZeyaScenarioExecuting(typeof(RuleDescription).Assembly)
            .AddZeyaLocalServerApiClients()

            .BuildServiceProvider();

        Formatter = _serviceProvider.GetRequiredService<XmlDocumentSyntaxFormatter>();
        RepositoryProvider = _serviceProvider.GetRequiredService<LocalRepositoryProvider>();
        SolutionFileStructureBuilderFactory = _serviceProvider.GetRequiredService<SolutionFileStructureBuilderFactory>();
    }

    public LocalGithubRepository CreateGithubRepository()
    {
        return RepositoryProvider.GetGithubRepository("owner", "name");
    }

    public ILocalRepository CreateLocalRepository()
    {
        return RepositoryProvider.GetLocalRepository(CurrentPath, LocalRepositorySolutionManager.DefaultMask);
    }

    public ScenarioContext CreateGithubRepositoryValidationScenarioContext()
    {
        LocalGithubRepository localGithubRepository = CreateGithubRepository();
        var repositoryValidationContext = new RepositoryValidationContext(localGithubRepository, DiagnosticCollectorAsserts.GetCollector());
        return RepositoryValidationContextExtensions.CreateScenarioContext(repositoryValidationContext);
    }

    public ScenarioContext CreateLocalRepositoryValidationScenarioContext()
    {
        var localRepository = CreateLocalRepository();
        var repositoryValidationContext = new RepositoryValidationContext(localRepository, DiagnosticCollectorAsserts.GetCollector());
        return RepositoryValidationContextExtensions.CreateScenarioContext(repositoryValidationContext);
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public ILogger<T> GetLogger<T>()
    {
        return _serviceProvider.GetRequiredService<ILogger<T>>();
    }
}