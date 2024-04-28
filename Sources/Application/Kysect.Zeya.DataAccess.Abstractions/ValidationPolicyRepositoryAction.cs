namespace Kysect.Zeya.DataAccess.Abstractions;

public class ValidationPolicyRepositoryAction(Guid actionId, Guid validationPolicyRepositoryId, string title, DateTimeOffset performedAt)
{
    public Guid ActionId { get; init; } = actionId;
    public Guid ValidationPolicyRepositoryId { get; init; } = validationPolicyRepositoryId;
    public string Title { get; init; } = title;
    public DateTimeOffset PerformedAt { get; init; } = performedAt;

}