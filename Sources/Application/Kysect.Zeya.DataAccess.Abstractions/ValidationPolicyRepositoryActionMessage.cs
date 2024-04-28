namespace Kysect.Zeya.DataAccess.Abstractions;

public class ValidationPolicyRepositoryActionMessage(Guid actionId, string message)
{
    public Guid ActionId { get; init; } = actionId;
    public string Message { get; init; } = message;
}