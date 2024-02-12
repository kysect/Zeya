using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.Abstractions.Models;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tests.IntegrationManager;

public class RepositoryValidationServiceTests : IDisposable
{
    private readonly RepositoryValidationRuleProvider _repositoryValidationRuleProvider;
    private readonly RepositoryValidationService _repositoryValidationService;
    private readonly TestTemporaryDirectory _temporaryDirectory;
    private readonly GithubRepositoryProvider _githubRepositoryProvider;

    public RepositoryValidationServiceTests()
    {
        ILogger logger = TestLoggerProvider.GetLogger();
        var fileSystem = new FileSystem();

        _repositoryValidationRuleProvider = RepositoryValidationRuleProviderTestInstance.Create();
        _temporaryDirectory = new TestTemporaryDirectory(fileSystem);
        string repositoriesDirectory = _temporaryDirectory.GetTemporaryDirectory();
        var localStoragePathFactory = new FakePathFormatStrategy(repositoriesDirectory);
        IGithubIntegrationService githubIntegrationService = FakeGithubIntegrationServiceTestInstance.Create(localStoragePathFactory);

        IServiceCollection serviceCollection = new ServiceCollection()
            .AddSingleton(logger)
            .AddZeyaDotnetProjectSystemIntegration()
            .AddZeyaValidationRules()
            .AddZeyaValidationRuleFixers()
            .AddSingleton(githubIntegrationService);

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        IValidationRuleFixerApplier validationRuleFixerApplier = serviceProvider.GetRequiredService<IValidationRuleFixerApplier>();
        IScenarioStepHandler scenarioStepHandler = serviceProvider.GetRequiredService<IScenarioStepHandler>();

        _repositoryValidationService = new RepositoryValidationService(
            _repositoryValidationRuleProvider,
            new RepositoryValidator(logger, scenarioStepHandler),
            new LoggerRepositoryValidationReporter(logger),
            new RepositoryDiagnosticFixer(validationRuleFixerApplier, logger),
            new GitIntegrationService(null),
            githubIntegrationService,
            new PullRequestMessageCreator(),
            logger);

        _githubRepositoryProvider = new GithubRepositoryProvider(fileSystem, logger, githubIntegrationService, localStoragePathFactory);
    }

    [Fact]
    public void Analyze_ReturnExpectedResult()
    {
        LocalGithubRepository localGithubRepository = _githubRepositoryProvider.GetGithubRepository("Kysect", "Zeya");
        RepositoryValidationReport repositoryValidationReport = _repositoryValidationService.Analyze([localGithubRepository], "ValidationScenario.yaml");
    }

    [Fact]
    public void AnalyzerAndFix_ReturnExpectedResult()
    {
        LocalGithubRepository localGithubRepository = _githubRepositoryProvider.GetGithubRepository("Kysect", "Zeya");
        _repositoryValidationService.AnalyzerAndFix(localGithubRepository, "ValidationScenario.yaml");
    }

    public void Dispose()
    {
        _temporaryDirectory.Dispose();
    }
}