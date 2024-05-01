using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.DependencyManager;

public class GitIntegrationCredentialOptions
{
    [Required]
    public GitCredentialType AuthType { get; init; } = GitCredentialType.UserPassword;
    [Required]
    public string GithubUsername { get; init; } = null!;
    [Required]
    public string GithubToken { get; init; } = null!;
}