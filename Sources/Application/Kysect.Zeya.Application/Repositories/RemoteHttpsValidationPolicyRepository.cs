using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Exceptions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.Dtos;

namespace Kysect.Zeya.Application.Repositories;

public class RemoteHttpsValidationPolicyRepository : IValidationPolicyRepository
{
    private readonly ValidationPolicyRepository _info;

    public Guid Id => _info.Id;
    public Guid ValidationPolicyId => _info.ValidationPolicyId;
    public ValidationPolicyRepositoryType Type => _info.Type;
    public string SolutionPathMask => _info.SolutionPathMask;

    public string RemoteHttpsUrl { get; }

    public RemoteHttpsValidationPolicyRepository(ValidationPolicyRepository info)
    {
        info.ThrowIfNull();

        if (info.Type != ValidationPolicyRepositoryType.RemoteHttps)
            throw SwitchDefaultExceptions.OnUnexpectedValue(info.Type);

        _info = info;

        RemoteHttpsUrl = info.Metadata;
    }

    public ValidationPolicyRepositoryDto ToDto()
    {
        return new ValidationPolicyRepositoryDto(_info.Id, _info.ValidationPolicyId, _info.Metadata, _info.Type.ToString());
    }
}