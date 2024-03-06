namespace Kysect.Zeya.Dtos;

public record ValidationPolicyDto(Guid Id, string Name, string Content);
public record ValidationPolicyRepositoryDto(Guid Id, Guid ValidationPolicyId, string Name);