using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya.RepositoryValidation;

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
        return new ScenarioContext() { Data = validationContext };
    }
}