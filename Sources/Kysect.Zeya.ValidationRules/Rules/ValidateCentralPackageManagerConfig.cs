using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;
using Kysect.Zeya.ProjectSystemIntegration;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Rules;

[ScenarioStep("ValidateCentralPackageManagerConfig")]
public record ValidateCentralPackageManagerConfig(string MasterFile) : IScenarioStep
{
    public static string DiagnosticCode = "SRC00004";
    public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;

    public static string GetMessageMissedConfigurationFile(ValidateCentralPackageManagerConfig request)
    {
        return $"Configuration file Directory.Package.props for Central Package Management is missed.";
    }

    public static string GetMessageAboutMissed(ValidateCentralPackageManagerConfig request, NugetVersion nugetVersion)
    {
        return $"Nuget {nugetVersion.PackageName} version is not specified in master Directory.Package.props";
    }

    public static string GetMessageAboutDifferent(ValidateCentralPackageManagerConfig request, NugetVersion currentVersion, string masterVersions)
    {
        return $"Nuget {currentVersion.PackageName} should be {masterVersions} but was {currentVersion.Version}";
    }
}

public class ValidateCentralPackageManagerConfigValidationRule : IScenarioStepExecutor<ValidateCentralPackageManagerConfig>
{
    private readonly IFileSystem _fileSystem;
    private readonly DirectoryPackagesParser _directoryPackagesParser;
    private readonly ILogger _logger;

    public ValidateCentralPackageManagerConfigValidationRule(IFileSystem fileSystem, DirectoryPackagesParser directoryPackagesParser, ILogger logger)
    {
        _fileSystem = fileSystem;
        _directoryPackagesParser = directoryPackagesParser;
        _logger = logger;
    }

    public void Execute(ScenarioContext context, ValidateCentralPackageManagerConfig request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        if (!_fileSystem.File.Exists(request.MasterFile))
        {
            _logger.LogError("Master file {File} for checking CPM was not found.", request.MasterFile);
            return;
        }

        var masterFileContent = _fileSystem.File.ReadAllText(request.MasterFile);
        var masterPackages = _directoryPackagesParser
            .Parse(masterFileContent)
            .ToDictionary(p => p.PackageName, p => p.Version);
        
        // TODO: Move to constants?
        var currentProjectConfiguration = Path.Combine(repositoryValidationContext.RepositoryAccessor.GetFullPath(), "Sources", "Directory.Packages.props");
        if (!_fileSystem.File.Exists(currentProjectConfiguration))
        {
            _logger.LogError("Repository does not contains Directory.Packages.props file");
            return;
        }

        var currentProjectDirectoryPackage = _fileSystem.File.ReadAllText(currentProjectConfiguration);
        var currentProjectPackages = _directoryPackagesParser.Parse(currentProjectDirectoryPackage);

        foreach (var nugetVersion in currentProjectPackages)
        {
            if (!masterPackages.TryGetValue(nugetVersion.PackageName, out var masterVersion))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    ValidateCentralPackageManagerConfig.DiagnosticCode,
                    ValidateCentralPackageManagerConfig.GetMessageAboutMissed(request, nugetVersion),
                    ValidateCentralPackageManagerConfig.DefaultSeverity);
                continue;
            }

            if (nugetVersion.Version != masterVersion)
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    ValidateCentralPackageManagerConfig.DiagnosticCode,
                    ValidateCentralPackageManagerConfig.GetMessageAboutDifferent(request, nugetVersion, masterVersion),
                    ValidateCentralPackageManagerConfig.DefaultSeverity);
            }
        }
    }
}