using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules;

[ScenarioStep("LicenseFileExists")]
public record LicenseFileExists() : IScenarioStep
{
    public static string DiagnosticCode = "SRC00005";
    public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;

    public static string GetMessage(LicenseFileExists request)
    {
        return $"License file was not found.";
    }
}

public class LicenseFileExistsValidationRule : IScenarioStepExecutor<LicenseFileExists>
{
    private readonly IFileSystem _fileSystem;

    public LicenseFileExistsValidationRule(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void Execute(ScenarioContext context, LicenseFileExists request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        if (!_fileSystem.File.Exists("LICENSE"))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                LicenseFileExists.DiagnosticCode,
                LicenseFileExists.GetMessage(request),
                LicenseFileExists.DefaultSeverity);
        }
    }
}