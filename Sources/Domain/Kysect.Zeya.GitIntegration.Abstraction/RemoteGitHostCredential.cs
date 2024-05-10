using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.GitIntegration.Abstraction;

public class RemoteGitHostCredential
{
    [Required]
    public string HostType { get; init; } = null!;
    [Required]
    public GitCredentialType AuthType { get; init; } = GitCredentialType.UserPassword;
    [Required]
    public string Username { get; } = null!;
    [Required]
    public string Token { get; init; } = null!;
    public string? HostUrl { get; init; } = null!;
}
