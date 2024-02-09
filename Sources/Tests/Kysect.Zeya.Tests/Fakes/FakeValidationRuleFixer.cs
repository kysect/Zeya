using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ValidationRules.Abstractions;

namespace Kysect.Zeya.Tests.Fakes;

public class FakeValidationRuleFixer : IValidationRuleFixer<FakeValidationRule>
{
    public int FixCalls { get; private set; }

    public void Fix(FakeValidationRule rule, IClonedRepository clonedRepository)
    {
        FixCalls++;
    }
}