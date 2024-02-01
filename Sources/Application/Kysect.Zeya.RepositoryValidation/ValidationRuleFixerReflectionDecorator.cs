using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;
using Kysect.Zeya.Abstractions.Contracts;
using System;
using System.Reflection;

namespace Kysect.Zeya.RepositoryValidation;

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

    public void Execute(IValidationRule rule, IClonedRepository clonedRepository)
    {
        _executeMethod.Invoke(_fixer, [rule, clonedRepository]);
    }
}