using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya.ValidationRules.Abstractions;

public interface IValidationRule : IScenarioStep
{
    string DiagnosticCode { get; }
}