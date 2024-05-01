namespace Kysect.Zeya.DataAccess.Abstractions;

public class ValidationPolicyRepositoryActionMessage(Guid actionMessageId, Guid actionId, string message)
{
    public Guid ActionMessageId { get; init; } = actionMessageId;
    public Guid ActionId { get; init; } = actionId;
    public string Message { get; init; } = message;
}