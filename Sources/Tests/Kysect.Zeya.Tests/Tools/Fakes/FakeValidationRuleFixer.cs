using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.ValidationRules.Abstractions;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public class FakeValidationRuleFixer : IValidationRuleFixer<FakeValidationRule>
{
    public int FixCalls { get; private set; }

    public void Fix(FakeValidationRule rule, ILocalRepository localRepository)
    {
        FixCalls++;
    }
}