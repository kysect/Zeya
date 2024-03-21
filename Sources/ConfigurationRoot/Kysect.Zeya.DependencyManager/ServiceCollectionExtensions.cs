using Kysect.CommonLib.DependencyInjection;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.ScenarioLib;
using Kysect.ScenarioLib.Abstractions;
using Kysect.ScenarioLib.YamlParser;
using Kysect.TerminalUserInterface.Commands;
using Kysect.TerminalUserInterface.DependencyInjection;
using Kysect.TerminalUserInterface.Navigation;
using Kysect.Zeya.Application;
using Kysect.Zeya.Application.LocalHandling;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules;
using Kysect.Zeya.Tui;
using Kysect.Zeya.Tui.Controls;
using Kysect.Zeya.WebApiClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using Serilog;
using Serilog.Extensions.Logging;
using Spectre.Console;
using System.IO.Abstractions;
using System.Reflection;

namespace Kysect.Zeya.DependencyManager;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppSettingsConfiguration(this IServiceCollection serviceCollection)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        return serviceCollection
            .AddSingleton(config);
    }

    public static IServiceCollection AddZeyaConfiguration(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddOptionsWithValidation<GithubIntegrationOptions>("GithubIntegrationOptions");
    }

    public static IServiceCollection AddZeyaHttpService(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddZeyaRefit(new Uri("http://apiservice"));
    }

    public static IServiceCollection AddZeyaLocalHandlingService(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddZeyaDotnetProjectSystemIntegration()
            .AddSingleton<IGitIntegrationService>(sp => new GitIntegrationService(sp.GetRequiredService<IOptions<GithubIntegrationOptions>>().Value.CommitAuthor))
            .AddZeyaGithubIntegration()
            .AddZeyaValidationRulesAndFixers()
            .AddZeyaScenarioExecuting()
            .AddZeyaRepositoryValidation()
            .AddZeyaLocalServerApiClients();
    }

    public static IServiceCollection AddZeyaConsoleLogging(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddLogging(b =>
            {
                b
                    .AddFilter(null, LogLevel.Trace)
                    .AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = false;
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss";
                    })
                    .AddProvider(
                        new SerilogLoggerProvider(
                            new LoggerConfiguration()
                                .WriteTo.File(path: "Zeya.log", rollingInterval: RollingInterval.Day)
                                .MinimumLevel.Verbose()
                                .CreateLogger()));
            });
    }

    public static IServiceCollection AddZeyaSqliteDbContext(this IServiceCollection serviceCollection, string databaseName = ":memory:")
    {
        return serviceCollection
            .AddDbContextFactory<ZeyaDbContext>(options =>
            {
                options.UseSqlite($"Data Source={databaseName}");
            });
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
            .AddSingleton<ValidationRuleParser>()
            .AddSingleton<RepositoryValidator>()
            .AddSingleton<RepositoryDiagnosticFixer>()
            .AddSingleton<PullRequestMessageCreator>()
            .AddScoped<RepositoryValidationService>()
            .AddScoped<ValidationPolicyRepositoryFactory>()
            .AddScoped<ValidationPolicyService>();

        return serviceCollection;
    }

    public static IServiceCollection AddZeyaScenarioExecuting(this IServiceCollection serviceCollection)
    {
        Assembly[] validationRuleAssembly = new[]
        {
            typeof(RuleDescription).Assembly
        };

        // TODO: customize scenario directory path

        return serviceCollection
            .AddSingleton<IScenarioContentProvider>(sp => new ScenarioContentProvider(sp.GetRequiredService<IFileSystem>(), "Samples"))
            .AddSingleton<IScenarioContentParser, YamlScenarioContentParser>()
            .AddSingleton<IScenarioContentStepDeserializer, ScenarioContentStepReflectionDeserializer>(_ => ScenarioContentStepReflectionDeserializer.Create(validationRuleAssembly))
            .AddSingleton<IScenarioContentDeserializer, ScenarioContentDeserializer>()
            .AddSingleton<IScenarioStepHandler, ScenarioStepReflectionHandler>(sp => ScenarioStepReflectionHandler.Create(sp, validationRuleAssembly));
    }

    public static IServiceCollection AddZeyaValidationRulesAndFixers(this IServiceCollection serviceCollection)
    {
        Assembly[] validationRuleFixerAssembly = new[]
        {
            typeof(RuleDescription).Assembly
        };

        return serviceCollection
            .AddAllImplementationOf<IScenarioStepExecutor>(validationRuleFixerAssembly)
            .AddAllImplementationOf<IValidationRuleFixer>(validationRuleFixerAssembly)
            .AddSingleton<IValidationRuleFixerApplier>(sp => ValidationRuleFixerApplier.Create(sp, validationRuleFixerAssembly));
    }

    public static IServiceCollection AddZeyaTerminalUserInterface(this IServiceCollection serviceCollection)
    {
        Assembly[] consoleCommandAssemblies = new[] { typeof(IRootMenu).Assembly };

        serviceCollection
            .AddSingleton(AnsiConsole.Console)
            .AddScoped<PolicySelectorControl>()
            .AddScoped<RepositoryNameInputControl>()
            .AddUserActionSelectionMenus(consoleCommandAssemblies)
            .AddSingleton(CreateUserActionSelectionMenuNavigator);

        return serviceCollection;
    }

    public static IServiceCollection AddZeyaLocalServerApiClients(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<IPolicyService, PolicyService>()
            .AddScoped<IPolicyRepositoryService, PolicyRepositoryService>()
            .AddScoped<IPolicyValidationService, PolicyValidationService>()
            .AddScoped<IPolicyRepositoryValidationService, PolicyRepositoryValidationService>();
    }

    private static TuiMenuNavigator CreateUserActionSelectionMenuNavigator(IServiceProvider serviceProvider)
    {
        ILogger<TuiMenuNavigator> logger = serviceProvider.GetRequiredService<ILogger<TuiMenuNavigator>>();
        ICommandExecutor commandExecutor = serviceProvider.GetRequiredService<ICommandExecutor>();
        return TuiMenuNavigator.Create<IRootMenu>(commandExecutor, logger);
    }
}