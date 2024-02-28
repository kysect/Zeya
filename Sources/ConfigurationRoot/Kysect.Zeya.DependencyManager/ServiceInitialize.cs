using Kysect.Zeya.DataAccess.EntityFramework;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.DependencyManager;

public static class ServiceInitialize
{
    public static async Task InitializeDatabase(IServiceProvider serviceProvider)
    {
        using IServiceScope serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<ZeyaDbContext>();
        await context.Database.EnsureCreatedAsync();
    }
}