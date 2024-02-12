﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.ValidationRules.Abstractions;
using Kysect.Zeya.ValidationRules.Rules.Github;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Fixers.Github;

public class GithubWorkflowEnabledValidationRuleFixer(IFileSystem fileSystem, ILogger logger) : IValidationRuleFixer<GithubWorkflowEnabledValidationRule.Arguments>
{
    public void Fix(GithubWorkflowEnabledValidationRule.Arguments rule, ILocalRepository localRepository)
    {
        rule.ThrowIfNull();
        localRepository.ThrowIfNull();

        if (localRepository is not LocalGithubRepository clonedGithubRepository)
        {
            logger.LogError("Cannot apply github validation rule on non github repository");
            return;
        }

        IFileInfo masterFileInfo = fileSystem.FileInfo.New(rule.MasterFile);
        string workflowPath = clonedGithubRepository.GetWorkflowPath(masterFileInfo.Name);

        logger.LogInformation("Copy workflow from {Source} to {Target}", rule.MasterFile, workflowPath);
        string masterFileContent = fileSystem.File.ReadAllText(rule.MasterFile);
        localRepository.FileSystem.WriteAllText(workflowPath, masterFileContent);
    }
}