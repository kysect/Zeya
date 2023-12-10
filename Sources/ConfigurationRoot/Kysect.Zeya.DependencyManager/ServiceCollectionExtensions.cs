﻿using Kysect.CommonLib.DependencyInjection;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.GithubUtils.RepositorySync;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.GithubIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System.IO.Abstractions;
using System.Reflection;
using Kysect.ScenarioLib;
using Kysect.ScenarioLib.Abstractions;
using Kysect.ScenarioLib.YamlParser;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ManagedDotnetCli;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.ValidationRules.Fixers;
using Kysect.TerminalUserInterface.DependencyInjection;
using Kysect.TerminalUserInterface.Navigation;
using Kysect.Zeya.Tui;

namespace Kysect.Zeya.DependencyManager;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZeyaRequiredService(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddZeyaConfiguration()
            .AddZeyaLogging()
            .AddZeyaGithubIntegration()
            .AddZeyaRepositoryValidation();
    }

    public static IServiceCollection AddZeyaConfiguration(this IServiceCollection serviceCollection)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        return serviceCollection
            .AddSingleton(config)
            .AddOptionsWithValidation<GithubIntegrationOptions>("GithubIntegrationOptions");
    }

    public static IServiceCollection AddZeyaLogging(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton(_ => CreateLogger());
    }

    public static IServiceCollection AddZeyaGithubIntegration(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IPathFormatStrategy>(sp =>
        {
            var githubIntegrationOptions = sp.GetRequiredService<IOptions<GithubIntegrationOptions>>();
            return new UseOwnerAndRepoForFolderNameStrategy(githubIntegrationOptions.Value.CacheDirectoryPath);
        });

        serviceCollection.AddSingleton<IGitHubClient>(sp =>
        {
            var githubIntegrationOptions = sp.GetRequiredService<IOptions<GithubIntegrationOptions>>();
            return new GitHubClient(new ProductHeaderValue("Zeya")) { Credentials = new Credentials(githubIntegrationOptions.Value.GithubToken) };
        });

        return serviceCollection
            .AddSingleton<IFileSystem, FileSystem>()
            .AddSingleton<IGithubRepositoryProvider, GithubRepositoryProvider>()
            .AddSingleton<IGithubIntegrationService, GithubIntegrationService>();
    }

    public static IServiceCollection AddZeyaRepositoryValidation(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<DotnetCli>()
            .AddSingleton<DotnetSolutionModifierFactory>()
            .AddSingleton<IDotnetProjectPropertyAccessor, DotnetProjectPropertyAccessor>();

        serviceCollection.AddSingleton<DirectoryPackagesParser>();
        serviceCollection.AddSingleton<IRepositoryValidationReporter, LoggerRepositoryValidationReporter>();
        serviceCollection.AddSingleton<RepositoryValidator>();
        serviceCollection = serviceCollection
            .AddZeyaValidationRules()
            .AddZeyaValidationRuleFixers();

        return serviceCollection;
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

    private static IServiceCollection AddZeyaValidationRules(this IServiceCollection serviceCollection)
    {
        Assembly[] validationRuleAssembly = new[]
        {
            typeof(RepositoryValidationContext).Assembly
        };

        // TODO: customize scenario directory path

        return serviceCollection
            .AddSingleton<IScenarioSourceProvider>(sp => new ScenarioSourceProvider("Samples"))
            .AddSingleton<IScenarioSourceCodeParser, YamlScenarioSourceCodeParser>()
            .AddSingleton<IScenarioStepParser, ScenarioStepReflectionParser>(_ => ScenarioStepReflectionParser.Create(validationRuleAssembly))
            .AddAllImplementationOf<IScenarioStepExecutor>(validationRuleAssembly)
            .AddSingleton<IScenarioStepHandler, ScenarioStepReflectionHandler>(sp => ScenarioStepReflectionHandler.Create(sp, validationRuleAssembly));
    }

    private static IServiceCollection AddZeyaValidationRuleFixers(this IServiceCollection serviceCollection)
    {
        Assembly[] validationRuleFixerAssembly = new[]
        {
            typeof(RepositoryValidationContext).Assembly
        };

        return serviceCollection
            .AddAllImplementationOf<IValidationRuleFixer>(validationRuleFixerAssembly)
            .AddSingleton<IValidationRuleFixerApplier>(sp => ValidationRuleFixerApplier.Create(sp, validationRuleFixerAssembly));
    }

    public static IServiceCollection AddZeyaTerminalUserInterface(this IServiceCollection serviceCollection)
    {
        Assembly[] consoleCommandAssemblies = new[] { typeof(TuiMenuInitializer).Assembly };

        serviceCollection
            .AddUserActionSelectionMenus(consoleCommandAssemblies);
        serviceCollection.AddSingleton(CreateUserActionSelectionMenuNavigator);

        return serviceCollection;
    }

    private static TuiMenuNavigator CreateUserActionSelectionMenuNavigator(IServiceProvider serviceProvider)
    {
        ILogger logger = serviceProvider.GetRequiredService<ILogger>();

        var userActionSelectionMenuProvider = new TuiMenuProvider(serviceProvider);
        var userActionSelectionMenuInitializer = new TuiMenuInitializer(userActionSelectionMenuProvider);
        TuiMenuNavigationItem selectionMenuNavigatorItem = userActionSelectionMenuInitializer.CreateMenu();
        return new TuiMenuNavigator(selectionMenuNavigatorItem, logger);
    }
}