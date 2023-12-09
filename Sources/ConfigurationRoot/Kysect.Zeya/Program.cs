using Kysect.TerminalUserInterface.Navigation;
using Kysect.Zeya.DependencyManager;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya;

public class Program
{
    public static void Main(string[] args)
    {
        var serviceProvider = BuildServiceProvider();
        var tuiMenuNavigator = serviceProvider.GetRequiredService<TuiMenuNavigator>();
        tuiMenuNavigator.Run();
    }

    public static IServiceProvider BuildServiceProvider()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection
            .AddZeyaRequiredService()
            .AddZeyaTerminalUserInterface();

        return serviceCollection.BuildServiceProvider();
    }
}