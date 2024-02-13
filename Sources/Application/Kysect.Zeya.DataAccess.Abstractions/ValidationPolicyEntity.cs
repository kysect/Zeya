using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.DataAccess.Abstractions;

public class ValidationPolicyEntity
{
    [Key]
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Content { get; init; }

    public ValidationPolicyEntity(Guid id, string name, string content)
    {
        Id = id;
        Name = name;
        Content = content;
    }
}