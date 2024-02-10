using Kysect.Zeya.GitIntegration.Abstraction;
using System.IO.Abstractions;

namespace Kysect.Zeya.GithubIntegration.Abstraction;

public class ClonedGithubRepository(GithubRepositoryName githubMetadata, string repositoryRootPath, IFileSystem fileSystem)
    : ClonedRepository(repositoryRootPath, fileSystem)
{
    public GithubRepositoryName GithubMetadata { get; } = githubMetadata;
}