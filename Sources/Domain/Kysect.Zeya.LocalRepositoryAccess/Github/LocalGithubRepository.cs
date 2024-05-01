using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.Zeya.GithubIntegration.Abstraction;
using System.IO.Abstractions;

namespace Kysect.Zeya.LocalRepositoryAccess.Github;

public class LocalGithubRepository(
    GithubRepositoryName githubMetadata,
    string repositoryRootPath,
    string solutionSearchMask,
    IGithubIntegrationService githubIntegrationService,
    IFileSystem fileSystem,
    DotnetSolutionModifierFactory solutionModifierFactory)
    : ILocalRepository
{
    public GithubRepositoryName GithubMetadata => githubMetadata;
    public LocalRepositoryFileSystem FileSystem => new LocalRepositoryFileSystem(repositoryRootPath, fileSystem);
    public LocalRepositorySolutionManager SolutionManager { get; } = new LocalRepositorySolutionManager(repositoryRootPath, solutionSearchMask, fileSystem, solutionModifierFactory);
    public IGithubIntegrationService GitHubIntegrationService { get; } = githubIntegrationService;

    public string GetRepositoryName()
    {
        return GithubMetadata.FullName;
    }

    public string GetWorkflowPath(string workflowName)
    {
        return fileSystem.Path.Combine(".github", "workflows", workflowName);
    }
}