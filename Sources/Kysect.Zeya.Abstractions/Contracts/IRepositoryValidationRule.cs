using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IRepositoryValidationRule<in TRepository>
{
    string Name { get; }
    RepositoryValidationReport Validate(TRepository repository);
}