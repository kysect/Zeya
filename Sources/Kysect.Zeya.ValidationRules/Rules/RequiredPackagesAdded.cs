using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ProjectSystemIntegration;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Rules;

[ScenarioStep("RequiredPackagesAdded")]
public record RequiredPackagesAdded(IReadOnlyCollection<string> Packages) : IScenarioStep
{
    public static string DiagnosticCode = "SRC00008";
    public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;

    public static string GetMessage(RequiredPackagesAdded request, string package)
    {
        return $"Package {package} is not add to all solution.";
    }
}

public class RequiredPackagesAddedValidationRule : IScenarioStepExecutor<RequiredPackagesAdded>
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public RequiredPackagesAddedValidationRule(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public void Execute(ScenarioContext context, RequiredPackagesAdded request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        var filePath = Path.Combine(repositoryValidationContext.RepositoryAccessor.GetFullPath(), "Sources", "Directory.Build.props");
        if (!_fileSystem.File.Exists(filePath))
        {
            _logger.LogError("Directory.Build.props file is missed.");
            return;
        }

        var directoryBuildProps = _fileSystem.File.ReadAllText(filePath);
        var parser = new DirectoryBuildPropsParser();
        var addedPackages = parser.GetListOfPackageReference(directoryBuildProps).ToHashSet();

        foreach (var requestPackage in request.Packages)
        {
            if (!addedPackages.Contains(requestPackage))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    RequiredPackagesAdded.DiagnosticCode,
                    RequiredPackagesAdded.GetMessage(request, requestPackage),
                    RequiredPackagesAdded.DefaultSeverity);
            }
        }
    }
}