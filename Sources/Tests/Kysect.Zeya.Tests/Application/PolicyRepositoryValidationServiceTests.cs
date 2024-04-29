using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Fix;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tests.Application;

public class PolicyRepositoryValidationServiceTests : IDisposable
{
    private readonly ValidationRuleParser _validationRuleParser;
    private readonly TestTemporaryDirectory _temporaryDirectory;
    private readonly LocalRepositoryProvider _localRepositoryProvider;
    private readonly IScenarioContentProvider _scenarioProvider;
    private readonly RepositoryValidationProcessingAction _repositoryValidator;
    private readonly RepositoryFixProcessingAction _repositoryDiagnosticFixer;

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
        _repositoryValidator = serviceProvider.GetRequiredService<RepositoryValidationProcessingAction>();
        var repositoryValidationReporter = serviceProvider.GetRequiredService<IRepositoryValidationReporter>();
        _repositoryDiagnosticFixer = serviceProvider.GetRequiredService<RepositoryFixProcessingAction>();
        IRepositoryFetcher repositoryFetcher = serviceProvider.GetRequiredService<IRepositoryFetcher>();
        _scenarioProvider = RepositoryValidationRuleProviderTestInstance.CreateContentProvider(fileSystem);

        _localRepositoryProvider = new LocalRepositoryProvider(
            fileSystem,
            serviceProvider.GetRequiredService<ILogger<LocalRepositoryProvider>>(),
            githubIntegrationService,
            localStoragePathFactory,
            new DotnetSolutionModifierFactory(fileSystem, new SolutionFileContentParser(), new XmlDocumentSyntaxFormatter()),
            repositoryFetcher);
    }

    [Fact]
    public void Analyze_ReturnExpectedResult()
    {
        LocalGithubRepository localGithubRepository = _localRepositoryProvider.GetGithubRepository("Kysect", "Zeya");
        string scenarioContent = _scenarioProvider.GetScenarioSourceCode("ValidationScenario.yaml");
        IReadOnlyCollection<IValidationRule> validationRules = _validationRuleParser.GetValidationRules(scenarioContent);
        RepositoryValidationReport repositoryValidationReport = _repositoryValidator.Process(localGithubRepository, new RepositoryValidationProcessingAction.Request(validationRules));

    }

    [Fact]
    public void AnalyzerAndFix_ReturnExpectedResult()
    {
        LocalGithubRepository localGithubRepository = _localRepositoryProvider.GetGithubRepository("Kysect", "Zeya");
        string scenarioContent = _scenarioProvider.GetScenarioSourceCode("ValidationScenario.yaml");
        IReadOnlyCollection<IValidationRule> validationRules = _validationRuleParser.GetValidationRules(scenarioContent);
        RepositoryValidationReport repositoryValidationReport = _repositoryValidator.Process(localGithubRepository, new RepositoryValidationProcessingAction.Request(validationRules));
        _repositoryDiagnosticFixer.Process(localGithubRepository, new RepositoryFixProcessingAction.Request(validationRules, repositoryValidationReport.GetAllDiagnosticRuleCodes()));
    }

    public void Dispose()
    {
        _temporaryDirectory.Dispose();
    }
}