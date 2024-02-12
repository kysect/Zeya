using Kysect.Zeya.RepositoryValidation;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public class FakeValidationRule : IValidationRule
{
    public string DiagnosticCode => "CODE0001";
}