using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.ScenarioLib.Abstractions;
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

public class PolicyRepositoryValidationServiceTests : IDisposable
{
    private readonly ValidationRuleParser _validationRuleParser;
    private readonly RepositoryValidationService _policyRepositoryValidationService;
    private readonly TestTemporaryDirectory _temporaryDirectory;
    private readonly GithubRepositoryProvider _githubRepositoryProvider;
    private readonly IScenarioContentProvider _scenarioProvider;

    public PolicyRepositoryValidationServiceTests()
    {
        var fileSystem = new FileSystem();

        _validationRuleParser = RepositoryValidationRuleProviderTestInstance.Create();
        _temporaryDirectory = new TestTemporaryDirectory(fileSystem);
        string repositoriesDirectory = _temporaryDirectory.GetTemporaryDirectory();
        var localStoragePathFactory = new FakePathFormatStrategy(repositoriesDirectory);
        IGithubIntegrationService githubIntegrationService = FakeGithubIntegrationServiceTestInstance.Create(localStoragePathFactory);

        IServiceCollection serviceCollection = new ServiceCollection()
            .AddZeyaTestLogging()
            .AddZeyaDotnetProjectSystemIntegration()
            .AddZeyaScenarioExecuting()
            .AddZeyaValidationRulesAndFixers()
            .AddZeyaRepositoryValidation()
            .AddSingleton(githubIntegrationService)
            .AddSingleton<IRepositoryFetcher>(sp =>
            {
                ILogger<IRepositoryFetcher> logger = sp.GetRequiredService<ILogger<IRepositoryFetcher>>();
                var repositoryFetchOptions = RepositoryFetchOptions.CreateWithUserPasswordAuth("GithubUsername", "GithubToken");
                return new RepositoryFetcher(repositoryFetchOptions, logger);
            });

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        var repositoryValidator = serviceProvider.GetRequiredService<RepositoryValidator>();
        var repositoryValidationReporter = serviceProvider.GetRequiredService<IRepositoryValidationReporter>();
        var repositoryDiagnosticFixer = serviceProvider.GetRequiredService<RepositoryDiagnosticFixer>();
        IRepositoryFetcher repositoryFetcher = serviceProvider.GetRequiredService<IRepositoryFetcher>();
        _scenarioProvider = RepositoryValidationRuleProviderTestInstance.CreateContentProvider(fileSystem);

        _policyRepositoryValidationService = new RepositoryValidationService(
            _validationRuleParser,
            repositoryValidator,
            repositoryValidationReporter,
            repositoryDiagnosticFixer,
            new GitIntegrationService(null),
            githubIntegrationService,
            new PullRequestMessageCreator(),
            serviceProvider.GetRequiredService<ILogger<RepositoryValidationService>>());

        _githubRepositoryProvider = new GithubRepositoryProvider(
            fileSystem,
            serviceProvider.GetRequiredService<ILogger<GithubRepositoryProvider>>(),
            githubIntegrationService,
            localStoragePathFactory,
            new DotnetSolutionModifierFactory(fileSystem, new SolutionFileContentParser(), new XmlDocumentSyntaxFormatter()),
            repositoryFetcher);
    }

    [Fact]
    public void Analyze_ReturnExpectedResult()
    {
        LocalGithubRepository localGithubRepository = _githubRepositoryProvider.GetGithubRepository("Kysect", "Zeya");
        string scenarioContent = _scenarioProvider.GetScenarioSourceCode("ValidationScenario.yaml");
        RepositoryValidationReport repositoryValidationReport = _policyRepositoryValidationService.Analyze([localGithubRepository], scenarioContent);
    }

    [Fact]
    public void AnalyzerAndFix_ReturnExpectedResult()
    {
        LocalGithubRepository localGithubRepository = _githubRepositoryProvider.GetGithubRepository("Kysect", "Zeya");
        string scenarioContent = _scenarioProvider.GetScenarioSourceCode("ValidationScenario.yaml");
        _policyRepositoryValidationService.AnalyzerAndFix(localGithubRepository, scenarioContent);
    }

    public void Dispose()
    {
        _temporaryDirectory.Dispose();
    }
}