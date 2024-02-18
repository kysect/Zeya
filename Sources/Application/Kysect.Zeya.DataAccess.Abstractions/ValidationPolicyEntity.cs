using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.DataAccess.Abstractions;

public class ValidationPolicyEntity(Guid id, string name, string content)
{
    [Key]
    public Guid Id { get; init; } = id;

    public string Name { get; init; } = name;
    public string Content { get; init; } = content;
}