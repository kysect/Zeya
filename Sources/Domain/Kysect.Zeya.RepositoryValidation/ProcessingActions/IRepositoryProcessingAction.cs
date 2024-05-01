using Kysect.Zeya.LocalRepositoryAccess;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions;

public interface IRepositoryProcessingAction<TRequest, TResponse>
{
    RepositoryProcessingResponse<TResponse> Process(ILocalRepository repository, TRequest request);
}

public record RepositoryProcessingResponse<T>(
    string ActionName,
    T Value,
    IReadOnlyCollection<RepositoryProcessingMessage> Messages);

public record RepositoryProcessingMessage(string Code, string Message, RepositoryValidationSeverity Severity);