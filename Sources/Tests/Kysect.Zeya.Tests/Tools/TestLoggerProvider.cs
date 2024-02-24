using Kysect.CommonLib.DependencyInjection.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tests.Tools;

public static class TestLoggerProvider
{
    public static ILogger GetLogger()
    {
        return DefaultLoggerConfiguration.CreateConsoleLogger();
    }

    public static IServiceCollection AddZeyaTestLogging(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddLogging(b =>
            {
                b
                    .AddFilter(null, LogLevel.Trace)
                    .AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = false;
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss";
                    });
            });
    }

    public static ILogger<T> GetLogger<T>()
    {
        return new ServiceCollection()
            .AddZeyaTestLogging()
            .BuildServiceProvider()
            .GetRequiredService<ILogger<T>>();
    }
}