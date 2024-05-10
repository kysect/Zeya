using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.GitIntegration.Abstraction;

public class GitCommitAuthor
{
    [Required]
    public string AuthorName { get; init; } = null!;
    [Required]
    public string AuthorMail { get; init; } = null!;
}