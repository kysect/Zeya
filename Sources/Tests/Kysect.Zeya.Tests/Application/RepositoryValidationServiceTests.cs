using Kysect.Zeya.Application;
using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tests.Application;

public class RepositoryValidationServiceTests : IDisposable
{
    private readonly RepositoryValidationRuleProvider _repositoryValidationRuleProvider;
    private readonly RepositoryValidationService _repositoryValidationService;
    private readonly TestTemporaryDirectory _temporaryDirectory;
    private readonly GithubRepositoryProvider _githubRepositoryProvider;

    public RepositoryValidationServiceTests()
    {
        var fileSystem = new FileSystem();

        _repositoryValidationRuleProvider = RepositoryValidationRuleProviderTestInstance.Create();
        _temporaryDirectory = new TestTemporaryDirectory(fileSystem);
        string repositoriesDirectory = _temporaryDirectory.GetTemporaryDirectory();
        var localStoragePathFactory = new FakePathFormatStrategy(repositoriesDirectory);
        IGithubIntegrationService githubIntegrationService = FakeGithubIntegrationServiceTestInstance.Create(localStoragePathFactory);

        IServiceCollection serviceCollection = new ServiceCollection()
            .AddZeyaTestLogging()
            .AddZeyaDotnetProjectSystemIntegration()
            .AddZeyaValidationRules()
            .AddZeyaValidationRuleFixers()
            .AddZeyaRepositoryValidation()
            .AddSingleton(githubIntegrationService);

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        var repositoryValidator = serviceProvider.GetRequiredService<RepositoryValidator>();
        var repositoryValidationReporter = serviceProvider.GetRequiredService<IRepositoryValidationReporter>();
        var repositoryDiagnosticFixer = serviceProvider.GetRequiredService<RepositoryDiagnosticFixer>();

        _repositoryValidationService = new RepositoryValidationService(
            _repositoryValidationRuleProvider,
            repositoryValidator,
            repositoryValidationReporter,
            repositoryDiagnosticFixer,
            new GitIntegrationService(null),
            githubIntegrationService,
            new PullRequestMessageCreator(),
            serviceProvider.GetRequiredService<ILogger<RepositoryValidationService>>());

        _githubRepositoryProvider = new GithubRepositoryProvider(fileSystem, serviceProvider.GetRequiredService<ILogger<GithubRepositoryProvider>>(), githubIntegrationService, localStoragePathFactory);
    }

    [Fact]
    public void Analyze_ReturnExpectedResult()
    {
        LocalGithubRepository localGithubRepository = _githubRepositoryProvider.GetGithubRepository("Kysect", "Zeya");
        ScenarioContent scenarioContent = _repositoryValidationRuleProvider.ReadScenario("ValidationScenario.yaml");
        RepositoryValidationReport repositoryValidationReport = _repositoryValidationService.Analyze([localGithubRepository], scenarioContent);
    }

    [Fact]
    public void AnalyzerAndFix_ReturnExpectedResult()
    {
        LocalGithubRepository localGithubRepository = _githubRepositoryProvider.GetGithubRepository("Kysect", "Zeya");
        ScenarioContent scenarioContent = _repositoryValidationRuleProvider.ReadScenario("ValidationScenario.yaml");
        _repositoryValidationService.AnalyzerAndFix(localGithubRepository, scenarioContent);
    }

    public void Dispose()
    {
        _temporaryDirectory.Dispose();
    }
}