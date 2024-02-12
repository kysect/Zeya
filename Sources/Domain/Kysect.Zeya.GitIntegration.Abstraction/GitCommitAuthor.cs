using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.GitIntegration.Abstraction;

public class GitCommitAuthor
{
    [Required]
    public string GithubUsername { get; init; } = null!;
    [Required]
    public string GithubMail { get; init; } = null!;
}