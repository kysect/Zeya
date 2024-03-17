﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.RepositoryValidation;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;

namespace Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;

public class SourcesMustNotBeInRootValidationRule(IFileSystem fileSystem) : IScenarioStepExecutor<SourcesMustNotBeInRootValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.SourcesMustNotBeInRoot")]
    public record Arguments(
        [property: Required] string SourceDirectoryPath) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.SourcesMustNotBeInRoot;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
        public const string DirectoryMissedMessage = "Directory for sources was not found in repository";
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        string repositoryRootPath = repositoryValidationContext.Repository.FileSystem.GetFullPath();

        var expectedSourceDirectoryPath = fileSystem.Path.Combine(repositoryRootPath, request.SourceDirectoryPath);
        if (!fileSystem.Directory.Exists(expectedSourceDirectoryPath))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                Arguments.DirectoryMissedMessage,
                Arguments.DefaultSeverity);
            return;
        }

        var solutionsInRoot = fileSystem.Directory
            .EnumerateFiles(repositoryRootPath, "*.sln", SearchOption.TopDirectoryOnly)
            .Where(p => !p.StartsWith(expectedSourceDirectoryPath))
            .ToList();

        if (solutionsInRoot.Any())
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Sources must be located in {expectedSourceDirectoryPath}. Founded solution files: {solutionsInRoot.ToSingleString()}",
                Arguments.DefaultSeverity);
        }
    }
}