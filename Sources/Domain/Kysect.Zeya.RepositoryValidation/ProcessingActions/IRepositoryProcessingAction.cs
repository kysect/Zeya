using Kysect.Zeya.LocalRepositoryAccess;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions;

public interface IRepositoryProcessingAction<TRequest, TResponse>
{
    TResponse Process(ILocalRepository repository, TRequest request);
}