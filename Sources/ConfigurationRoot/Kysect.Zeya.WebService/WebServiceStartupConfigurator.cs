using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.DependencyManager;
using System.IO.Abstractions;

namespace Kysect.Zeya.WebService;

public class WebServiceStartupConfigurator
{
    public async Task Config(WebApplication app)
    {
        app.ThrowIfNull();

        using IServiceScope serviceScope = app.Services.CreateScope();

        // TODO: this is hack for preventing error in case when service run before postgresql is ready. Need to rework this is future
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        await ServiceInitialize.InitializeDatabase(serviceScope.ServiceProvider);
        await AddDefaultPolicy(serviceScope);
    }

    private async Task AddDefaultPolicy(IServiceScope serviceScope)
    {
        var zeyaDbContext = serviceScope.ServiceProvider.GetRequiredService<ZeyaDbContext>();
        IFileSystem fileSystem = serviceScope.ServiceProvider.GetRequiredService<IFileSystem>();
        string demoScenarioContent = await fileSystem.File.ReadAllTextAsync("Demo-validation.yaml");

        var demoPolicy = new ValidationPolicyEntity(Guid.NewGuid(), "Demo policy", demoScenarioContent);
        zeyaDbContext.ValidationPolicies.Add(demoPolicy);
        await zeyaDbContext.SaveChangesAsync();
    }
}