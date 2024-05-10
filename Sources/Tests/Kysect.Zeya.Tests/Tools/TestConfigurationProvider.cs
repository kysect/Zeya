using Microsoft.Extensions.Configuration;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tests.Tools;

public static class TestConfigurationProvider
{
    private static readonly FileSystem _fileSystem = new FileSystem();

    public static IConfiguration Create()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(_fileSystem.Path.Combine("DependencyManager", "appsettings.json"))
            .Build();
        return config;
    }
}