using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryAccess;
using System.IO.Abstractions;

namespace Kysect.Zeya.GithubIntegration.Abstraction;

public class ClonedGithubRepositoryAccessor(GithubRepository githubMetadata, string repositoryRootPath, IFileSystem fileSystem) : ClonedRepositoryAccessor(repositoryRootPath, fileSystem)
{
    public GithubRepository GithubMetadata { get; } = githubMetadata;
}