using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class SourcesMustNotBeInRootValidationRule(IFileSystem fileSystem) : IScenarioStepExecutor<SourcesMustNotBeInRootValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.SourcesMustNotBeInRoot")]
    public record Arguments() : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.SourcesMustNotBeInRoot;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();
        var repositoryRootPath = repositoryValidationContext.Repository.GetFullPath();

        var expectedSourceDirectoryPath = fileSystem.Path.Combine(repositoryRootPath, ValidationConstants.DefaultSourceCodeDirectory);
        if (!fileSystem.Directory.Exists(expectedSourceDirectoryPath))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"Directory for sources was not found in repository",
                Arguments.DefaultSeverity);
            return;
        }

        var solutionsInRoot = fileSystem.Directory.EnumerateFiles(repositoryRootPath, "*.sln", SearchOption.TopDirectoryOnly).ToList();
        if (solutionsInRoot.Any())
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"Sources must not located in root of repository. Founded solution files: {solutionsInRoot.ToSingleString()}",
                Arguments.DefaultSeverity);
        }
    }
}