using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Kysect.Zeya.DataAccess.EntityFramework.Tools;

public static class DbSetExtensionMethods
{
    public static async ValueTask<T> GetAsync<T>(this DbSet<T> dbSet, params object?[] keyValues) where T : class
    {
        dbSet.ThrowIfNull();
        keyValues.ThrowIfNull();

        T? result = await dbSet.FindAsync();
        return result ?? throw new ArgumentException($"Cannot find {typeof(T)} by key {keyValues.ToSingleString()}");
    }
}