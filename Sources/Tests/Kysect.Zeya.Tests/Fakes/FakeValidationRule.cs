using Kysect.Zeya.Abstractions.Contracts;

namespace Kysect.Zeya.Tests.Fakes;

public class FakeValidationRule : IValidationRule
{
    public string DiagnosticCode => "CODE0001";
}