﻿using Kysect.CommonLib.DependencyInjection;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.PowerShellRunner.Configuration;
using Kysect.ScenarioLib;
using Kysect.ScenarioLib.Abstractions;
using Kysect.ScenarioLib.YamlParser;
using Kysect.TerminalUserInterface.DependencyInjection;
using Kysect.TerminalUserInterface.Navigation;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tui;
using Kysect.Zeya.Tui.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System.IO.Abstractions;
using System.Reflection;

namespace Kysect.Zeya.DependencyManager;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZeyaConfiguration(this IServiceCollection serviceCollection)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        return serviceCollection
            .AddSingleton(config)
            .AddOptionsWithValidation<GithubIntegrationOptions>("GithubIntegrationOptions");
    }

    public static IServiceCollection AddZeyaRequiredService(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddZeyaLogging()
            .AddPowerShellWrappers()
            .AddZeyaDotnetProjectSystemIntegration()
            .AddSingleton<IGitIntegrationService>(sp => new GitIntegrationService(sp.GetRequiredService<IOptions<GithubIntegrationOptions>>().Value.CommitAuthor))
            .AddZeyaGithubIntegration()
            .AddZeyaValidationRules()
            .AddZeyaValidationRuleFixers()
            .AddZeyaRepositoryValidation();
    }

    public static IServiceCollection AddZeyaLogging(this IServiceCollection serviceCollection)
    {
        // TODO: disable IncludeScopes in default implementation PredefinedLogger.CreateConsoleLogger
        using var logConfigurationBuilder = new LogConfigurationBuilder();
        const LogLevel logLevel = LogLevel.Trace;

        ILogger logger = logConfigurationBuilder
            .SetLevel(logLevel)
            .SetRedirectToAppData("Kysect", "Zeya")
            .SetDefaultCategory("Zeya")
            .AddSpectreConsole()
            .AddSerilogToFile("Zeya.log")
            .Build();

        return serviceCollection.AddSingleton(logger);
    }

    private static IServiceCollection AddPowerShellWrappers(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddPowerShellLogger(b =>
            {
                b
                    .SetDefaultCategory("Cloudext")
                    .SetRedirectToAppData("GreenCloud", "Cloudext")
                    .SetLevel(LogLevel.Trace)
                    .AddSerilogToFile("Cloudext.log");
            })
            .AddPowerShellAccessorFactory()
            .AddPowerShellAccessor();
    }

    public static IServiceCollection AddZeyaGithubIntegration(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(sp =>
        {
            IOptions<GithubIntegrationOptions> githubOptions = sp.GetRequiredService<IOptions<GithubIntegrationOptions>>();
            return githubOptions.Value.Credential;
        });

        serviceCollection.AddSingleton<ILocalStoragePathFactory>(sp =>
        {
            var githubIntegrationOptions = sp.GetRequiredService<IOptions<GithubIntegrationOptions>>();
            return new UseOwnerAndRepoForFolderNameStrategy(githubIntegrationOptions.Value.CacheDirectoryPath);
        });

        serviceCollection.AddSingleton<IGitHubClient>(sp =>
        {
            var credentials = sp.GetRequiredService<GithubIntegrationCredential>();
            return new GitHubClient(new ProductHeaderValue("Zeya")) { Credentials = new Credentials(credentials.GithubToken) };
        });

        return serviceCollection
            .AddSingleton<IGithubIntegrationService, GithubIntegrationService>()
            .AddSingleton<IGithubRepositoryProvider, GithubRepositoryProvider>();
    }

    public static IServiceCollection AddZeyaDotnetProjectSystemIntegration(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IFileSystem, FileSystem>()
            .AddSingleton<XmlDocumentSyntaxFormatter>()
            .AddSingleton<DotnetSolutionModifierFactory>()
            .AddSingleton<SolutionFileContentParser>();
    }

    public static IServiceCollection AddZeyaRepositoryValidation(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<IRepositoryValidationReporter, LoggerRepositoryValidationReporter>()
            .AddSingleton<RepositoryValidationRuleProvider>()
            .AddSingleton<RepositoryValidator>()
            .AddSingleton<RepositoryDiagnosticFixer>()
            .AddSingleton<PullRequestMessageCreator>()
            .AddSingleton<RepositoryValidationService>();

        return serviceCollection;
    }

    public static IServiceCollection AddZeyaValidationRules(this IServiceCollection serviceCollection)
    {
        Assembly[] validationRuleAssembly = new[]
        {
            typeof(RuleDescription).Assembly
        };

        // TODO: customize scenario directory path

        return serviceCollection
            .AddSingleton<IScenarioSourceProvider>(sp => new ScenarioSourceProvider(sp.GetRequiredService<IFileSystem>(), "Samples"))
            .AddSingleton<IScenarioSourceCodeParser, YamlScenarioSourceCodeParser>()
            .AddSingleton<IScenarioStepParser, ScenarioStepReflectionParser>(_ => ScenarioStepReflectionParser.Create(validationRuleAssembly))
            .AddAllImplementationOf<IScenarioStepExecutor>(validationRuleAssembly)
            .AddSingleton<IScenarioStepHandler, ScenarioStepReflectionHandler>(sp => ScenarioStepReflectionHandler.Create(sp, validationRuleAssembly));
    }

    public static IServiceCollection AddZeyaValidationRuleFixers(this IServiceCollection serviceCollection)
    {
        Assembly[] validationRuleFixerAssembly = new[]
        {
            typeof(RuleDescription).Assembly
        };

        return serviceCollection
            .AddAllImplementationOf<IValidationRuleFixer>(validationRuleFixerAssembly)
            .AddSingleton<IValidationRuleFixerApplier>(sp => ValidationRuleFixerApplier.Create(sp, validationRuleFixerAssembly));
    }

    public static IServiceCollection AddZeyaTerminalUserInterface(this IServiceCollection serviceCollection)
    {
        Assembly[] consoleCommandAssemblies = new[] { typeof(TuiMenuInitializer).Assembly };

        serviceCollection
            .AddSingleton<RepositorySelectorControl>()
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