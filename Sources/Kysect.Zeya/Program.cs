using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya;

public class Program
{
    public static void Main(string[] args)
    {
        var dependencyManager = new DependencyManager();
        var serviceProvider = dependencyManager.BuildServiceProvider();
        var demoScenario = serviceProvider.GetRequiredService<DemoScenario>();

        var validationRules = new IRepositoryValidationRule<GithubRepository>[]
        {

        };

        demoScenario.Process();
    }
}