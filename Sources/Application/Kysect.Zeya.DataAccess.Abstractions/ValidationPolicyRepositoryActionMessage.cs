namespace Kysect.Zeya.DataAccess.Abstractions;

public class ValidationPolicyRepositoryActionMessage(Guid repositoryActionId, string message)
{
    public Guid ActionId { get; init; } = repositoryActionId;
    public string Message { get; init; } = message;
}