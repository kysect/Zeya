using Kysect.Zeya.GitIntegration.Abstraction;
using System.IO.Abstractions;

namespace Kysect.Zeya.GithubIntegration.Abstraction;

public class ClonedGithubRepository(GithubRepositoryName githubMetadata, string repositoryRootPath, IFileSystem fileSystem)
    : ClonedRepository(repositoryRootPath, fileSystem)
{
    private readonly IFileSystem _fileSystem = fileSystem;

    public GithubRepositoryName GithubMetadata { get; } = githubMetadata;

    public override string GetRepositoryName()
    {
        return GithubMetadata.FullName;
    }

    public string GetWorkflowPath(string workflowName)
    {
        return _fileSystem.Path.Combine(".github", "workflows", workflowName);
    }
}