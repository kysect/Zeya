using Kysect.Zeya.GitIntegration.Abstraction;
using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.DependencyManager;

public class GithubIntegrationOptions
{
    public GitCommitAuthor? CommitAuthor { get; init; }

    [Required]
    public GitIntegrationCredential Credential { get; init; } = null!;
    [Required]
    public string CacheDirectoryPath { get; init; } = null!;
}