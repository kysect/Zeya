using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Exceptions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.GithubIntegration.Abstraction;

namespace Kysect.Zeya.Application.Repositories;

public class GithubValidationPolicyRepository : IValidationPolicyRepository
{
    private readonly ValidationPolicyRepository _info;

    public Guid Id => _info.Id;
    public Guid ValidationPolicyId => _info.ValidationPolicyId;
    public ValidationPolicyRepositoryType Type => _info.Type;
    public string SolutionPathMask => _info.SolutionPathMask;

    public string Owner { get; }
    public string Name { get; }

    public GithubValidationPolicyRepository(ValidationPolicyRepository info)
    {
        info.ThrowIfNull();

        if (info.Type != ValidationPolicyRepositoryType.Github)
            throw SwitchDefaultExceptions.OnUnexpectedValue(info.Type);

        _info = info;

        (string owner, string name) = GithubRepositoryName.Parse(info.Metadata);

        Owner = owner;
        Name = name;
    }

    public ValidationPolicyRepositoryDto ToDto()
    {
        return new ValidationPolicyRepositoryDto(_info.Id, _info.ValidationPolicyId, _info.Metadata);
    }
}