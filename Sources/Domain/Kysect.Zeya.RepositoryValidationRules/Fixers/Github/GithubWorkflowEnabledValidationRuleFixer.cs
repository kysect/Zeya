using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.Github;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.RepositoryValidationRules.Fixers.Github;

public class GithubWorkflowEnabledValidationRuleFixer(
    IFileSystem fileSystem,
    ILogger<GithubWorkflowEnabledValidationRuleFixer> logger) : IValidationRuleFixer<GithubWorkflowEnabledValidationRule.Arguments>
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

        foreach (string workflow in rule.Workflows)
        {
            IFileInfo masterFileInfo = fileSystem.FileInfo.New(workflow);
            string workflowPath = clonedGithubRepository.GetWorkflowPath(masterFileInfo.Name);

            logger.LogInformation("Copy workflow from {Source} to {Target}", workflow, workflowPath);
            string masterFileContent = fileSystem.File.ReadAllText(workflow);
            localRepository.FileSystem.WriteAllText(workflowPath, masterFileContent);
        }
    }
}