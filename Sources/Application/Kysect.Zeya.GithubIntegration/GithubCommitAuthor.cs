using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.GithubIntegration;

public class GithubCommitAuthor
{
    [Required]
    public string GithubUsername { get; init; } = null!;
    [Required]
    public string GithubMail { get; init; } = null!;
}