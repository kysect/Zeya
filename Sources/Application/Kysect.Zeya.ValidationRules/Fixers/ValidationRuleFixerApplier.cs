using System.Reflection;
using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules.Fixers;

public class ValidationRuleFixerApplier(Dictionary<string, IValidationRuleFixer> fixers) : IValidationRuleFixerApplier
{
    public static ValidationRuleFixerApplier Create(IServiceProvider serviceProvider, params Assembly[] assemblies)
    {
        serviceProvider.ThrowIfNull(nameof(serviceProvider));

        var fixers = new Dictionary<string, IValidationRuleFixer>();

        foreach (var type in AssemblyReflectionTraverser.GetAllImplementationOf<IValidationRuleFixer>(assemblies))
        {
            var service = serviceProvider.GetService(type);
            if (service == null)
                throw new ArgumentException($"Type {type.FullName} is not registered in DI");

            var validationRuleFixer = service.To<IValidationRuleFixer>();
            fixers[validationRuleFixer.DiagnosticCode] = validationRuleFixer;
        }

        return new ValidationRuleFixerApplier(fixers);
    }

    public bool IsFixerRegistered(string diagnosticCode)
    {
        return fixers.ContainsKey(diagnosticCode);
    }

    public void Apply(string diagnosticCode, IGithubRepositoryAccessor githubRepository)
    {
        if (!fixers.TryGetValue(diagnosticCode, out var fixer))
            throw new ArgumentException($"Fixer for {diagnosticCode} is not registered");

        fixer.Fix(githubRepository);
    }
}