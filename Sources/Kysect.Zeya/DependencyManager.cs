using System.IO.Abstractions;
using System.Reflection;
using Kysect.CommonLib.DependencyInjection;
using Kysect.GithubUtils.RepositorySync;
using Kysect.ScenarioLib;
using Kysect.ScenarioLib.Abstractions;
using Kysect.ScenarioLib.YamlParser;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.ManagedDotnetCli;
using Kysect.Zeya.ValidationRules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya;

public class DependencyManager
{
    public IServiceProvider BuildServiceProvider()
    {
        IServiceCollection serviceCollection = new ServiceCollection();

        ILogger logger = PredefinedLogger.CreateConsoleLogger("Zeya");
        var githubIntegrationOptions = new GithubIntegrationOptions()
        {
        };

        var pathFormatStrategy = new UseOwnerAndRepoForFolderNameStrategy(githubIntegrationOptions.CacheDirectoryPath);
        serviceCollection.AddSingleton(logger);
        serviceCollection.AddSingleton<IFileSystem, FileSystem>();
        serviceCollection.AddSingleton(githubIntegrationOptions);
        serviceCollection.AddSingleton<IPathFormatStrategy>(pathFormatStrategy);
        serviceCollection.AddSingleton<IGithubRepositoryProvider, GithubRepositoryProvider>();
        serviceCollection.AddSingleton<IGithubIntegrationService, GithubIntegrationService>();
        serviceCollection.AddSingleton<IRepositoryValidationReporter, LoggerRepositoryValidationReporter>();
        serviceCollection.AddSingleton<RepositoryValidator>();
        serviceCollection.AddSingleton<DotnetCli>();

        serviceCollection = AddScenarioExecution(serviceCollection);

        serviceCollection.AddSingleton<DemoScenario>();

        return serviceCollection.BuildServiceProvider();
    }

    public static IServiceCollection AddScenarioExecution(IServiceCollection serviceCollection)
    {
        Assembly[] assemblies = new[]
        {
            // Definition of assembly with IScenarioStep implementation
            // Definition of assembly with IScenarioStepExecutor implementation
            typeof(RepositoryValidationContext).Assembly
        };

        var dummyScenarioSourceProvider = new DummyScenarioSourceProvider();

        return serviceCollection
            .AddSingleton<IScenarioSourceProvider>(sp => dummyScenarioSourceProvider)
            .AddSingleton<IScenarioSourceCodeParser, YamlScenarioSourceCodeParser>()
            .AddSingleton<IScenarioStepParser, ScenarioStepReflectionParser>(_ => ScenarioStepReflectionParser.Create(assemblies))
            .AddAllImplementationOf<IScenarioStepExecutor>(assemblies)
            .AddSingleton<IScenarioStepHandler, ScenarioStepReflectionHandler>(sp => ScenarioStepReflectionHandler.Create(sp, assemblies));
    }
}