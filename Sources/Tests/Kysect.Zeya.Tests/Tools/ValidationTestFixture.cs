﻿using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Application;
using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Tools.Asserts;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Tools;

public class ValidationTestFixture
{
    private readonly IServiceProvider _serviceProvider;

    public XmlDocumentSyntaxFormatter Formatter { get; }
    public MockFileSystem FileSystem { get; }
    public string CurrentPath { get; }
    public GithubRepositoryProvider RepositoryProvider { get; }
    public SolutionFileStructureBuilderFactory SolutionFileStructureBuilderFactory { get; }

    public RepositoryDiagnosticCollectorAsserts DiagnosticCollectorAsserts { get; }
    public FileSystemAsserts FileSystemAsserts { get; }


    public ValidationTestFixture()
    {
        FileSystem = new MockFileSystem();
        CurrentPath = FileSystem.Path.GetFullPath(".");
        // TODO: get name from the repository
        DiagnosticCollectorAsserts = new RepositoryDiagnosticCollectorAsserts("MockRepository");
        FileSystemAsserts = new FileSystemAsserts(FileSystem);

        _serviceProvider = new ServiceCollection()
            .AddZeyaTestLogging()
            // TODO: Use same method with the Configuration root
            .AddSingleton<XmlDocumentSyntaxFormatter>()
            .AddSingleton<IFileSystem>(FileSystem)
            .AddSingleton<IGithubIntegrationService, DummyGithubIntegrationService>()
            .AddSingleton<ILocalStoragePathFactory>(new FakePathFormatStrategy(CurrentPath))
            .AddSingleton<GithubRepositoryProvider>()
            .AddSingleton<SolutionFileStructureBuilderFactory>()
            .AddSingleton<DotnetSolutionModifierFactory>()
            .AddSingleton<SolutionFileContentParser>()
            .AddZeyaValidationRulesAndFixers()
            .BuildServiceProvider();

        Formatter = _serviceProvider.GetRequiredService<XmlDocumentSyntaxFormatter>();
        RepositoryProvider = _serviceProvider.GetRequiredService<GithubRepositoryProvider>();
        SolutionFileStructureBuilderFactory = _serviceProvider.GetRequiredService<SolutionFileStructureBuilderFactory>();
    }

    public LocalGithubRepository CreateGithubRepository()
    {
        return RepositoryProvider.GetGithubRepository("owner", "name");
    }

    public ILocalRepository CreateLocalRepository()
    {
        return RepositoryProvider.GetLocalRepository(CurrentPath);
    }

    public ScenarioContext CreateGithubRepositoryValidationScenarioContext()
    {
        LocalGithubRepository localGithubRepository = CreateGithubRepository();
        var repositoryValidationContext = new RepositoryValidationContext(localGithubRepository, DiagnosticCollectorAsserts.GetCollector());
        return RepositoryValidationContextExtensions.CreateScenarioContext(repositoryValidationContext);
    }

    public ScenarioContext CreateLocalRepositoryValidationScenarioContext()
    {
        var localRepository = CreateLocalRepository();
        var repositoryValidationContext = new RepositoryValidationContext(localRepository, DiagnosticCollectorAsserts.GetCollector());
        return RepositoryValidationContextExtensions.CreateScenarioContext(repositoryValidationContext);
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public ILogger<T> GetLogger<T>()
    {
        return _serviceProvider.GetRequiredService<ILogger<T>>();
    }
}