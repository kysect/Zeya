using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya.RepositoryValidation;

public class RepositoryValidationRuleProvider(
    IScenarioSourceProvider scenarioProvider,
    IScenarioSourceCodeParser scenarioSourceCodeParser,
    IScenarioStepParser scenarioStepParser)
{
    public IReadOnlyCollection<IValidationRule> GetValidationRules(string validationScenarioName)
    {
        string scenarioSourceCode = scenarioProvider.GetScenarioSourceCode(validationScenarioName);
        IReadOnlyCollection<ScenarioStepArguments> scenarioStepNodes = scenarioSourceCodeParser.Parse(scenarioSourceCode);
        IReadOnlyCollection<IValidationRule> steps = scenarioStepNodes.Select(scenarioStepParser.ParseScenarioStep).Cast<IValidationRule>().ToList();
        return steps;
    }
}