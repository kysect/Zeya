using System.IO.Abstractions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class SourcesMustNotBeInRootValidationRule(IFileSystem fileSystem) : IScenarioStepExecutor<SourcesMustNotBeInRootValidationRule.SourceCodeDirectoryExists>
{
    [ScenarioStep("SourceCode.SourcesMustNotBeInRoot")]
    public record SourceCodeDirectoryExists(string ExpectedSourceDirectoryName) : IScenarioStep
    {
        public static string DiagnosticCode => RuleDescription.SourceCode.SourcesMustNotBeInRoot;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, SourceCodeDirectoryExists request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        var repositoryRootPath = repositoryValidationContext.RepositoryAccessor.GetFullPath();

        var solutionsInRoot = fileSystem.Directory.EnumerateFiles(repositoryRootPath, "*.sln", SearchOption.TopDirectoryOnly).ToList();
        if (solutionsInRoot.Any())
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                SourceCodeDirectoryExists.DiagnosticCode,
                $"Sources must not located in root of repository. Founded solution files: {solutionsInRoot.ToSingleString()}",
                SourceCodeDirectoryExists.DefaultSeverity);
        }

        var expectedSourceDirectoryPath = fileSystem.Path.Combine(repositoryRootPath, request.ExpectedSourceDirectoryName);

        if (!fileSystem.File.Exists(expectedSourceDirectoryPath))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                SourceCodeDirectoryExists.DiagnosticCode,
                $"Directory for sources was not found in repository",
                SourceCodeDirectoryExists.DefaultSeverity);
        }
    }
}