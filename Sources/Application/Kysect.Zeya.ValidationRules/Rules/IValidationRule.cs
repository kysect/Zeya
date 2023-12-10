using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules;

public interface IValidationRule : IScenarioStep
{
    string DiagnosticCode { get; }
}