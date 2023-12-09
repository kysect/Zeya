using System.Reflection;
using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;

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

    public bool Apply(string diagnosticCode, string repositoryPath)
    {
        if (!fixers.TryGetValue(diagnosticCode, out var fixer))
            return false;

        fixer.Fix(repositoryPath);
        return true;
    }
}