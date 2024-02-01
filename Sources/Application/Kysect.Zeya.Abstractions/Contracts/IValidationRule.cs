using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IValidationRule : IScenarioStep
{
    string DiagnosticCode { get; }
}