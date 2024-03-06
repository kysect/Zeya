using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.Dtos;

namespace Kysect.Zeya.Application.Repositories;

public class LocalValidationPolicyRepository : IValidationPolicyRepository
{
    private readonly ValidationPolicyRepository _info;

    public Guid Id => _info.Id;
    public Guid ValidationPolicyId => _info.ValidationPolicyId;
    public ValidationPolicyRepositoryType Type => _info.Type;

    public string Path => _info.Metadata;

    public LocalValidationPolicyRepository(ValidationPolicyRepository info)
    {
        _info = info.ThrowIfNull();
    }

    public ValidationPolicyRepositoryDto ToDto()
    {
        return new ValidationPolicyRepositoryDto(_info.Id, _info.ValidationPolicyId, _info.Metadata);
    }
}