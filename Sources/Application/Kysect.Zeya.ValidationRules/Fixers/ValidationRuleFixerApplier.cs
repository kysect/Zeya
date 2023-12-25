using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ValidationRules.Rules;
using System.Reflection;

namespace Kysect.Zeya.ValidationRules.Fixers;

public class ValidationRuleFixerApplier(Dictionary<Type, ValidationRuleFixerReflectionDecorator> fixers) : IValidationRuleFixerApplier
{
    private static readonly Type FixerType = typeof(IValidationRuleFixer<>);

    public static ValidationRuleFixerApplier Create(IServiceProvider serviceProvider, params Assembly[] assemblies)
    {
        serviceProvider.ThrowIfNull(nameof(serviceProvider));

        var fixers = new Dictionary<Type, ValidationRuleFixerReflectionDecorator>();

        // TODO: move this copy-paste part to reflection lib
        foreach (Type type in AssemblyReflectionTraverser.GetAllImplementationOf(assemblies, FixerType))
        {
            Type? fixerImplementation = AssemblyReflectionTraverser.FindInterfaceImplementationByGenericTypeDefinition(type, FixerType);
            if (fixerImplementation == null)
                continue;

            object? fixerInstance = serviceProvider.GetService(type);
            if (fixerInstance is null)
                throw new ArgumentException($"Fixer for type {type.FullName} is not registered in service provider.");

            Type argumentType = fixerImplementation.GetGenericArguments().Single();
            fixers.Add(argumentType, new ValidationRuleFixerReflectionDecorator(fixerInstance.To<IValidationRuleFixer>()));
        }

        return new ValidationRuleFixerApplier(fixers);
    }

    public bool IsFixerRegistered(IValidationRule rule)
    {
        rule.ThrowIfNull();

        return fixers.ContainsKey(rule.GetType());
    }

    public void Apply(IValidationRule rule, IClonedRepository clonedRepository)
    {
        rule.ThrowIfNull();

        if (!fixers.TryGetValue(rule.GetType(), out var fixer))
            throw new ArgumentException($"Fixer for {rule.DiagnosticCode} is not registered");

        fixer.Execute(rule, clonedRepository);
    }
}