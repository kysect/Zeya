using System.IO.Abstractions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules.Rules;

[ScenarioStep("SourceCodeDirectoryExists")]
public record SourceCodeDirectoryExists(string ExpectedSourceDirectoryName) : IScenarioStep
{
    public static string DiagnosticCode = "SRC00001";
    public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;

    public static string GetMessage(SourceCodeDirectoryExists request)
    {
        return $"The repository does not contain the expected directory {request.ExpectedSourceDirectoryName}";
    }
}

public class SourceCodeDirectoryExistsValidationRule : IScenarioStepExecutor<SourceCodeDirectoryExists>
{
    private readonly IFileSystem _fileSystem;

    public SourceCodeDirectoryExistsValidationRule(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void Execute(ScenarioContext context, SourceCodeDirectoryExists request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        var expectedPath = _fileSystem.Path.Combine(repositoryValidationContext.RepositoryAccessor.GetFullPath(), request.ExpectedSourceDirectoryName);

        if (!_fileSystem.File.Exists(expectedPath))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                SourceCodeDirectoryExists.DiagnosticCode,
                SourceCodeDirectoryExists.GetMessage(request),
                SourceCodeDirectoryExists.DefaultSeverity);
        }
    }
}