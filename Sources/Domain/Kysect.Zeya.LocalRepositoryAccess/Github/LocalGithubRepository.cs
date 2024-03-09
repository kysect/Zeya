using Kysect.Zeya.GithubIntegration.Abstraction;
using System.IO.Abstractions;

namespace Kysect.Zeya.LocalRepositoryAccess.Github;

public class LocalGithubRepository(GithubRepositoryName githubMetadata, string repositoryRootPath, string solutionSearchMask, IFileSystem fileSystem)
    : ILocalRepository
{
    public GithubRepositoryName GithubMetadata { get; } = githubMetadata;
    public LocalRepositoryFileSystem FileSystem => new LocalRepositoryFileSystem(repositoryRootPath, fileSystem);
    public LocalRepositorySolutionManager SolutionManager { get; } = new LocalRepositorySolutionManager(repositoryRootPath, solutionSearchMask, fileSystem);

    public string GetRepositoryName()
    {
        return GithubMetadata.FullName;
    }

    public string GetWorkflowPath(string workflowName)
    {
        return fileSystem.Path.Combine(".github", "workflows", workflowName);
    }
}