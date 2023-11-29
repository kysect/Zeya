using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;
using Kysect.Zeya.ManagedDotnetCli;

namespace Kysect.Zeya.ValidationRules.Rules;

[ScenarioStep("CentralPackageManagerEnabled")]
public record CentralPackageManagerEnabled(string ExpectedSourceDirectoryName) : IScenarioStep
{
    public static string DiagnosticCode = "SRC00002";
    public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;

    public static string GetMessage(CentralPackageManagerEnabled request)
    {
        return $"The repository does not use Central Package Manager";
    }
}

public class CentralPackageManagerEnabledValidationRule : IScenarioStepExecutor<CentralPackageManagerEnabled>
{
    private readonly IFileSystem _fileSystem;
    private readonly DotnetCli _dotnetCli;

    public CentralPackageManagerEnabledValidationRule(IFileSystem fileSystem, DotnetCli dotnetCli)
    {
        _fileSystem = fileSystem;
        _dotnetCli = dotnetCli;
    }

    public void Execute(ScenarioContext context, CentralPackageManagerEnabled request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        IEnumerable<string> projectFiles = _fileSystem.Directory.EnumerateFiles(repositoryValidationContext.RepositoryAccessor.GetFullPath(), "*.csproj", SearchOption.AllDirectories);

        var selectedProjectDefault = projectFiles.FirstOrDefault();
        if (selectedProjectDefault is null)
            return;

        var projectPropertyAccessor = new DotnetProjectPropertyAccessor(selectedProjectDefault, _dotnetCli);
        var managePackageVersionsCentrally = projectPropertyAccessor.ManagePackageVersionsCentrally();
        if (!managePackageVersionsCentrally)
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                CentralPackageManagerEnabled.DiagnosticCode,
                CentralPackageManagerEnabled.GetMessage(request),
                CentralPackageManagerEnabled.DefaultSeverity);
        }
    }
}