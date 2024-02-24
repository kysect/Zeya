using Kysect.Zeya.DataAccess.EntityFramework;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.DependencyManager;

public static class ServiceInitialize
{
    public static void InitializeDatabase(IServiceProvider serviceProvider)
    {
        using IServiceScope serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<ZeyaDbContext>();
        context.Database.EnsureCreated();
    }
}