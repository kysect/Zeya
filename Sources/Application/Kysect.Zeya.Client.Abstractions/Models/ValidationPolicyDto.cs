namespace Kysect.Zeya.Client.Abstractions.Models;

public record ValidationPolicyDto(Guid Id, string Name, string Content);
public record ValidationPolicyRepositoryDto(Guid Id, Guid ValidationPolicyId, string GithubOwner, string GithubRepository);