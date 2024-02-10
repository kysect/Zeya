using Kysect.Zeya.ValidationRules.Abstractions;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public class FakeValidationRule : IValidationRule
{
    public string DiagnosticCode => "CODE0001";
}