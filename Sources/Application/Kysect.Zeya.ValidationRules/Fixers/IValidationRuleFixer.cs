using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ValidationRules.Rules;
using System.Reflection;

namespace Kysect.Zeya.ValidationRules.Fixers;

public interface IValidationRuleFixer
{
}

public interface IValidationRuleFixer<TRules> : IValidationRuleFixer
    where TRules : IValidationRule
{
    void Fix(TRules rule, IGithubRepositoryAccessor githubRepository);
}

public class ValidationRuleFixerReflectionDecorator
{
    private const string FixMethodName = nameof(IValidationRuleFixer<IValidationRule>.Fix);

    private readonly IValidationRuleFixer _fixer;
    private readonly MethodInfo _executeMethod;

    public ValidationRuleFixerReflectionDecorator(IValidationRuleFixer fixer)
    {
        fixer.ThrowIfNull();

        Type executorType = fixer.GetType();
        MethodInfo? executeMethod = executorType.GetMethod(FixMethodName);

        _fixer = fixer;
        _executeMethod = executeMethod ?? throw new ReflectionException($"Cannot get method {executeMethod} from type {executorType}");
    }

    public void Execute(IValidationRule rule, IGithubRepositoryAccessor githubRepository)
    {
        _executeMethod.Invoke(_fixer, [rule, githubRepository]);
    }
}