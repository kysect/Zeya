using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;
using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya.ValidationRules;

public static class RepositoryValidationContextExtensions
{
    public static RepositoryValidationContext GetValidationContext(this ScenarioContext context)
    {
        context.ThrowIfNull();
        context.Data.ThrowIfNull();
        return context.Data.To<RepositoryValidationContext>();
    }

    public static ScenarioContext CreateScenarioContext(RepositoryValidationContext validationContext)
    {
        var initializer = new ReflectionInstanceInitializer<ScenarioContext>();
        initializer.Set("Data", validationContext);
        return initializer.GetValue();
    }
}