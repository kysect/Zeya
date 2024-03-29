﻿using Kysect.TerminalUserInterface.Navigation;
using Kysect.Zeya.DependencyManager;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya;

public static class Program
{
    public static void Main()
    {
        var serviceProvider = BuildServiceProvider();
        ServiceInitialize.InitializeDatabase(serviceProvider).Wait();
        var tuiMenuNavigator = serviceProvider.GetRequiredService<TuiMenuNavigator>();
        tuiMenuNavigator.Run();
    }

    public static IServiceProvider BuildServiceProvider()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection
            .AddAppSettingsConfiguration()
            .AddZeyaConfiguration()
            .AddZeyaConsoleLogging()
            .AddZeyaSqliteDbContext("Database.sql")
            .AddZeyaLocalHandlingService()
            .AddZeyaTerminalUserInterface();

        return serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
    }
}