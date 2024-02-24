using Kysect.TerminalUserInterface.Navigation;
using Kysect.Zeya.DependencyManager;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya;

public static class Program
{
    public static void Main()
    {
        var serviceProvider = BuildServiceProvider();
        ServiceInitialize.InitializeDatabase(serviceProvider);
        var tuiMenuNavigator = serviceProvider.GetRequiredService<TuiMenuNavigator>();
        tuiMenuNavigator.Run();
    }

    public static IServiceProvider BuildServiceProvider()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection
            .AddZeyaConfiguration()
            .AddZeyaLogging()
            .AddZeyaRequiredService()
            .AddZeyaTerminalUserInterface();

        return serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
    }
}