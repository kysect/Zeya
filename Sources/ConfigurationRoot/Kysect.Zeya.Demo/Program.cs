using Kysect.Zeya.DependencyManager;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.Demo
{
    internal class Program
    {
        static void Main()
        {
            var serviceProvider = BuildServiceProvider();
            var demoScenario = serviceProvider.GetRequiredService<DemoScenario>();

            demoScenario.Process();
        }

        public static IServiceProvider BuildServiceProvider()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddZeyaRequiredService();
            serviceCollection.AddSingleton<DemoScenario>();
            return serviceCollection.BuildServiceProvider();
        }
    }
}
