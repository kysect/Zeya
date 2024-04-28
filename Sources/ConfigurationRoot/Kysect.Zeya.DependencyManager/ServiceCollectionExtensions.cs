using Kysect.CommonLib.DependencyInjection;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
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
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions;
using Kysect.Zeya.RepositoryValidationRules.Rules;
using Kysect.Zeya.Tui;
using Kysect.Zeya.Tui.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    public static IServiceCollection AddZeyaLocalHandlingService(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddZeyaDotnetProjectSystemIntegration()
            .AddZeyaGitIntegration()
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
            .AddDbContext<ZeyaDbContext>(options =>
            {
                options.UseSqlite($"Data Source={databaseName}");
            });
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
            .AddSingleton<RepositoryValidationProcessingAction>()
            .AddSingleton<RepositoryFixProcessingAction>()
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