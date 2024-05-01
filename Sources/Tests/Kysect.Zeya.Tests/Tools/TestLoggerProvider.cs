using Kysect.CommonLib.DependencyInjection.Logging;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tests.Tools;

public static class TestLoggerProvider
{
    public static ILogger GetLogger()
    {
        return DefaultLoggerConfiguration.CreateConsoleLogger();
    }
}