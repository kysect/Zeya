﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;

namespace Kysect.Zeya.RepositoryValidationRules.Rules.Github;

public class GithubWorkflowEnabledValidationRule(IFileSystem fileSystem) : IScenarioStepExecutor<GithubWorkflowEnabledValidationRule.Arguments>
{
    [ScenarioStep("Github.BuildWorkflowEnabled")]
    public record Arguments(
        [property: Required] string MasterFile) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Github.BuildWorkflowEnabled;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();

        ILocalRepository localRepository = repositoryValidationContext.Repository;
        if (localRepository is not LocalGithubRepository clonedGithubRepository)
        {
            repositoryValidationContext.DiagnosticCollector.AddRuntimeError(
                request.DiagnosticCode,
                $"Cannot apply github validation rule on non github repository",
                Arguments.DefaultSeverity);
            return;
        }

        if (!fileSystem.File.Exists(request.MasterFile))
        {
            repositoryValidationContext.DiagnosticCollector.AddRuntimeError(
                request.DiagnosticCode,
                $"Master file {request.MasterFile} missed",
                Arguments.DefaultSeverity);
            return;
        }
        IFileInfo masterFileInfo = fileSystem.FileInfo.New(request.MasterFile);
        string masterFileContent = fileSystem.File.ReadAllText(request.MasterFile);

        var expectedPath = clonedGithubRepository.GetWorkflowPath(masterFileInfo.Name);
        if (!localRepository.FileSystem.Exists(expectedPath))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Workflow {masterFileInfo.Name} must be configured",
                Arguments.DefaultSeverity);
            return;
        }

        string repositoryWorkflowFileContent = localRepository.FileSystem.ReadAllText(expectedPath);
        if (!string.Equals(masterFileContent, repositoryWorkflowFileContent))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Workflow {masterFileInfo.Name} configuration do not match with master file",
                Arguments.DefaultSeverity);
        }
    }
}