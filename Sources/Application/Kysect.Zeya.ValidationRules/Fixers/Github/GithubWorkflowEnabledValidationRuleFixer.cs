using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ValidationRules.Abstractions;
using Kysect.Zeya.ValidationRules.Rules.Github;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules.Fixers.Github;

public class GithubWorkflowEnabledValidationRuleFixer(IFileSystem fileSystem, ILogger logger) : IValidationRuleFixer<GithubWorkflowEnabledValidationRule.Arguments>
{
    public void Fix(GithubWorkflowEnabledValidationRule.Arguments rule, IClonedRepository clonedRepository)
    {
        rule.ThrowIfNull();
        clonedRepository.ThrowIfNull();

        IFileInfo masterFileInfo = fileSystem.FileInfo.New(rule.MasterFile);
        string workflowPath = clonedRepository.GetWorkflowPath(masterFileInfo.Name);

        logger.LogInformation("Copy workflow from {Source} to {Target}", rule.MasterFile, workflowPath);
        string masterFileContent = fileSystem.File.ReadAllText(rule.MasterFile);
        clonedRepository.WriteAllText(workflowPath, masterFileContent);
    }
}