using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;
using Kysect.Zeya.ProjectSystemIntegration;

namespace Kysect.Zeya.ValidationRules.Rules;

[ScenarioStep("DirectoryBuildPropsContainsRequiredFields")]
public record DirectoryBuildPropsContainsRequiredFields(IReadOnlyCollection<string> RequiredFields) : IScenarioStep
{
    public static string DiagnosticCode = "SRC00007";
    public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;

    public static string GetMessage(DirectoryBuildPropsContainsRequiredFields request, string field)
    {
        return $"Directory.Build.props field {field} is missed.";
    }
}

public class DirectoryBuildPropsContainsRequiredFieldsValidationRule : IScenarioStepExecutor<DirectoryBuildPropsContainsRequiredFields>
{
    private readonly IFileSystem _fileSystem;

    public DirectoryBuildPropsContainsRequiredFieldsValidationRule(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void Execute(ScenarioContext context, DirectoryBuildPropsContainsRequiredFields request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        var filePath = Path.Combine(repositoryValidationContext.RepositoryAccessor.GetFullPath(), "Sources", "Directory.Build.props");

        var buildPropsValues = new Dictionary<string, string>();

        if (_fileSystem.File.Exists(filePath))
        {
            var directoryBuildPropsContent = _fileSystem.File.ReadAllText(filePath);
            var directoryBuildPropsParser = new DirectoryBuildPropsParser();
            buildPropsValues = directoryBuildPropsParser.Parse(directoryBuildPropsContent);
        }

        foreach (var requiredField in request.RequiredFields)
        {
            if (!buildPropsValues.ContainsKey(requiredField))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    DirectoryBuildPropsContainsRequiredFields.DiagnosticCode,
                    DirectoryBuildPropsContainsRequiredFields.GetMessage(request, requiredField),
                    DirectoryBuildPropsContainsRequiredFields.DefaultSeverity);
            }
        }
    }
}