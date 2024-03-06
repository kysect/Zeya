using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Exceptions;
using Kysect.Zeya.Common;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.Dtos;

namespace Kysect.Zeya.Application.Repositories;

public class GithubValidationPolicyRepository : IValidationPolicyRepository
{
    private readonly ValidationPolicyRepository _info;

    public Guid Id => _info.Id;
    public Guid ValidationPolicyId => _info.ValidationPolicyId;
    public ValidationPolicyRepositoryType Type => _info.Type;
    public string Owner { get; }
    public string Name { get; }

    public GithubValidationPolicyRepository(ValidationPolicyRepository info)
    {
        info.ThrowIfNull();

        if (info.Type != ValidationPolicyRepositoryType.Github)
            throw SwitchDefaultExceptions.OnUnexpectedValue(info.Type);

        _info = info;

        // TODO: Move this to GithubRepositoryName
        if (!info.Metadata.Contains('/'))
            throw new ZeyaException("Repository does not contains '/'");

        string[] parts = info.Metadata.Split('/', 2);
        Owner = parts[0];
        Name = parts[1];
    }

    public ValidationPolicyRepositoryDto ToDto()
    {
        return new ValidationPolicyRepositoryDto(_info.Id, _info.ValidationPolicyId, _info.Metadata);
    }
}