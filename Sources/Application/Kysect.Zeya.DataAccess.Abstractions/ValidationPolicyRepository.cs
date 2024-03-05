using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.DataAccess.Abstractions;

public class ValidationPolicyRepository(Guid id, Guid validationPolicyId, ValidationPolicyRepositoryType type, string metadata)
{
    [Key]
    public Guid Id { get; init; } = id;

    public Guid ValidationPolicyId { get; init; } = validationPolicyId;
    public ValidationPolicyRepositoryType Type { get; init; } = type;
    public string Metadata { get; set; } = metadata;
}
