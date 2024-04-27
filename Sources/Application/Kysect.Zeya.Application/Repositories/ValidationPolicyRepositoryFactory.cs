using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Exceptions;
using Kysect.Zeya.DataAccess.Abstractions;

namespace Kysect.Zeya.Application.Repositories;

public class ValidationPolicyRepositoryFactory
{
    public IValidationPolicyRepository Create(ValidationPolicyRepository info)
    {
        info.ThrowIfNull();

        return info.Type switch
        {
            ValidationPolicyRepositoryType.Github => new GithubValidationPolicyRepository(info),
            ValidationPolicyRepositoryType.Local => new LocalValidationPolicyRepository(info),
            ValidationPolicyRepositoryType.RemoteHttps => new RemoteHttpsValidationPolicyRepository(info),
            _ => throw SwitchDefaultExceptions.OnUnexpectedValue(info.Type)
        };
    }
}