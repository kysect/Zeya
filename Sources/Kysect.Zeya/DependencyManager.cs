using System.IO.Abstractions;
using System.Reflection;
using Kysect.CommonLib.DependencyInjection;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.GithubUtils.RepositorySync;
using Kysect.ScenarioLib;
using Kysect.ScenarioLib.Abstractions;
using Kysect.ScenarioLib.YamlParser;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.ManagedDotnetCli;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.ValidationRules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;

namespace Kysect.Zeya;

public class DependencyManager
{
    public IServiceProvider BuildServiceProvider()
    {
        IServiceCollection serviceCollection = new ServiceCollection();

        ILogger logger = CreateLogger();

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        serviceCollection.AddSingleton(config);
        serviceCollection.AddOptionsWithValidation<GithubIntegrationOptions>("GithubIntegrationOptions");

        serviceCollection.AddSingleton(logger);
        serviceCollection.AddSingleton<IFileSystem, FileSystem>();
        serviceCollection.AddSingleton<IPathFormatStrategy>(sp =>
        {
            var githubIntegrationOptions = sp.GetRequiredService<IOptions<GithubIntegrationOptions>>();
            return new UseOwnerAndRepoForFolderNameStrategy(githubIntegrationOptions.Value.CacheDirectoryPath);
        });
        serviceCollection.AddSingleton<IGithubRepositoryProvider, GithubRepositoryProvider>();
        serviceCollection.AddSingleton<IGithubIntegrationService, GithubIntegrationService>();
        serviceCollection.AddSingleton<IRepositoryValidationReporter, LoggerRepositoryValidationReporter>();
        serviceCollection.AddSingleton<RepositoryValidator>();
        serviceCollection.AddSingleton<DotnetCli>();
        serviceCollection.AddSingleton<IGitHubClient>(sp =>
        {
            var githubIntegrationOptions = sp.GetRequiredService<IOptions<GithubIntegrationOptions>>();
            return new GitHubClient(new ProductHeaderValue("Zeya")) { Credentials = new Credentials(githubIntegrationOptions.Value.GithubToken) };
        });

        serviceCollection = AddScenarioExecution(serviceCollection);

        serviceCollection.AddSingleton<DemoScenario>();
        serviceCollection.AddSingleton<DirectoryPackagesParser>();

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

        var dummyScenarioSourceProvider = new ScenarioSourceProvider("Samples");

        return serviceCollection
            .AddSingleton<IScenarioSourceProvider>(sp => dummyScenarioSourceProvider)
            .AddSingleton<IScenarioSourceCodeParser, YamlScenarioSourceCodeParser>()
            .AddSingleton<IScenarioStepParser, ScenarioStepReflectionParser>(_ => ScenarioStepReflectionParser.Create(assemblies))
            .AddAllImplementationOf<IScenarioStepExecutor>(assemblies)
            .AddSingleton<IScenarioStepHandler, ScenarioStepReflectionHandler>(sp => ScenarioStepReflectionHandler.Create(sp, assemblies));
    }

    // TODO: disable IncludeScopes in default implementation PredefinedLogger.CreateConsoleLogger
    private static ILogger CreateLogger(LogLevel logLevel = LogLevel.Trace)
    {
        using var logConfigurationBuilder = new LogConfigurationBuilder();

        return logConfigurationBuilder
            .SetLevel(logLevel)
            .SetDefaultCategory("Zeya")
            .AddSpectreConsole()
            .AddSerilogToFile("Zeya.log")
            .Build();
    }
}