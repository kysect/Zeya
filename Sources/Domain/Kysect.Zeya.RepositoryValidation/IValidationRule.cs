using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya.RepositoryValidation;

public interface IValidationRule : IScenarioStep
{
    string DiagnosticCode { get; }
}