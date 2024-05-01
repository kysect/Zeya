using Kysect.Zeya.DataAccess.EntityFramework;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.DependencyManager;

public static class ServiceInitialize
{
    public static async Task InitializeDatabase(IServiceProvider serviceProvider, bool userSqlite)
    {
        using IServiceScope serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<ZeyaDbContext>();
        if (userSqlite)
            await context.Database.EnsureDeletedAsync();

        await context.Database.EnsureCreatedAsync();
    }
}