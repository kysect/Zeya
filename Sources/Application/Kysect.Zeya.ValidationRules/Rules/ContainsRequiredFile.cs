using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules;

[ScenarioStep("ContainsRequiredFile")]
public record ContainsRequiredFile(string FilePath, string Sample) : IValidationRule
{
    public string DiagnosticCode => "SRC00005";
    public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
}

public class ContainsRequiredFileValidationRule : IScenarioStepExecutor<ContainsRequiredFile>
{
    private readonly IFileSystem _fileSystem;

    public ContainsRequiredFileValidationRule(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void Execute(ScenarioContext context, ContainsRequiredFile request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();

        var targetPath = Path.Combine(repositoryValidationContext.RepositoryAccessor.GetFullPath(), request.FilePath);

        if (!_fileSystem.File.Exists(targetPath))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"Required file {request.FilePath} was not found.",
                ContainsRequiredFile.DefaultSeverity);

            return;
        }

        var originalFile = _fileSystem.File.ReadAllText(targetPath);
        var sample = _fileSystem.File.ReadAllText(request.Sample);

        if (!string.Equals(originalFile, sample))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"Required file {request.FilePath} is not equal to sample.",
                ContainsRequiredFile.DefaultSeverity);
        }
    }
}