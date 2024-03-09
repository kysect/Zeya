using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.Dtos;

namespace Kysect.Zeya.Application.Repositories;

public interface IValidationPolicyRepository
{
    Guid Id { get; }
    Guid ValidationPolicyId { get; }
    ValidationPolicyRepositoryType Type { get; }
    string SolutionPathMask { get; }

    ValidationPolicyRepositoryDto ToDto();
}