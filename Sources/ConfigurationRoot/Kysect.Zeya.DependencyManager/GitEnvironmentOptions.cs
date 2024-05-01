using Kysect.Zeya.GitIntegration.Abstraction;
using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.DependencyManager;

public class GitEnvironmentOptions
{
    [Required]
    public string CacheDirectoryPath { get; init; } = null!;
    public GitCommitAuthor? CommitAuthor { get; init; }
}