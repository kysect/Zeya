using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.DataAccess.Abstractions;

public class ValidationPolicyRepository(Guid id, Guid validationPolicyId, string githubOwner, string githubRepository)
{
    [Key]
    public Guid Id { get; init; } = id;

    public Guid ValidationPolicyId { get; init; } = validationPolicyId;
    public string GithubOwner { get; init; } = githubOwner;
    public string GithubRepository { get; init; } = githubRepository;
}