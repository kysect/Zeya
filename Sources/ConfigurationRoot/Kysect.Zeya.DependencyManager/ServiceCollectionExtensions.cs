using Kysect.CommonLib.DependencyInjection;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.ScenarioLib;
using Kysect.ScenarioLib.Abstractions;
using Kysect.ScenarioLib.YamlParser;
using Kysect.Zeya.Application.DatabaseQueries;
using Kysect.Zeya.Application.LocalHandling;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.RepositoryDependencies;
using Kysect.Zeya.RepositoryDependencies.PackageDataCollecting;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.CreatePullRequest;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Fix;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using Kysect.Zeya.RepositoryValidationRules.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;
using System.Reflection;

namespace Kysect.Zeya.DependencyManager;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZeyaLocalHandlingService(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection
            .AddZeyaGitIntegration(configuration)
            .AddZeyaRemoteHostIntegration(configuration);

        return serviceCollection
            .AddSingleton<IFileSystem, FileSystem>()
            .AddZeyaDotnetProjectSystemIntegration()
            .AddZeyaValidationRulesAndFixers(typeof(RuleDescription).Assembly)
            .AddZeyaRepositoryValidation()
            .AddZeyaScenarioExecuting(typeof(RuleDescription).Assembly)
            .AddZeyaLocalServerApiClients();
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
            .AddSingleton<XmlDocumentSyntaxFormatter>()
            .AddSingleton<DotnetSolutionModifierFactory>()
            .AddSingleton<SolutionFileContentParser>();
    }

    public static IServiceCollection AddZeyaRepositoryValidation(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<LoggerRepositoryValidationReporter>()
            .AddSingleton<ValidationRuleParser>()
            .AddSingleton<RepositoryValidationProcessingAction>()
            .AddSingleton<RepositoryFixProcessingAction>()
            .AddSingleton<RepositoryCreatePullRequestProcessingAction>()
            .AddSingleton<PullRequestMessageCreator>()
            .AddScoped<ValidationPolicyRepositoryFactory>()
            .AddScoped<ValidationPolicyDatabaseQueries>();

        serviceCollection
            .AddSingleton<PackageRepositoryClient>()
            .AddSingleton<IPackageRepositoryClient, PackageRepositoryClient>()
            .AddSingleton<SolutionPackageDataCollector>()
            .AddSingleton<NugetPackageUpdateOrderBuilder>();

        return serviceCollection;
    }

    public static IServiceCollection AddZeyaScenarioExecuting(this IServiceCollection serviceCollection, Assembly assembly)
    {
        return serviceCollection
            .AddSingleton<IScenarioContentParser, YamlScenarioContentParser>()
            .AddSingleton<IScenarioContentStepDeserializer, ScenarioContentStepReflectionDeserializer>(_ => ScenarioContentStepReflectionDeserializer.Create(assembly))
            .AddSingleton<IScenarioContentDeserializer, ScenarioContentDeserializer>()
            .AddSingleton<IScenarioStepHandler, ScenarioStepReflectionHandler>(sp => ScenarioStepReflectionHandler.Create(sp, assembly));
    }

    public static IServiceCollection AddZeyaValidationRulesAndFixers(this IServiceCollection serviceCollection, Assembly assembly)
    {
        return serviceCollection
            .AddAllImplementationOf<IScenarioStepExecutor>(assembly)
            .AddAllImplementationOf<IValidationRuleFixer>(assembly)
            .AddSingleton<IValidationRuleFixerApplier>(sp => ValidationRuleFixerApplier.Create(sp, assembly));
    }

    public static IServiceCollection AddZeyaLocalServerApiClients(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<IPolicyService, PolicyService>()
            .AddScoped<IPolicyRepositoryService, PolicyRepositoryService>()
            .AddScoped<IPolicyValidationService, PolicyValidationService>()
            .AddScoped<IRepositoryDependenciesService, RepositoryDependenciesService>();
    }
}