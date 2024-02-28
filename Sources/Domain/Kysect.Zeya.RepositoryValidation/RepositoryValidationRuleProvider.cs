using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya.RepositoryValidation;

public class RepositoryValidationRuleProvider(
    IScenarioSourceProvider scenarioProvider,
    IScenarioSourceCodeParser scenarioSourceCodeParser,
    IScenarioStepParser scenarioStepParser)
{
    public ScenarioContent ReadScenario(string validationScenarioName)
    {
        string scenarioSourceCode = scenarioProvider.GetScenarioSourceCode(validationScenarioName);
        return new ScenarioContent(scenarioSourceCode);
    }

    public IReadOnlyCollection<IValidationRule> GetValidationRules(ScenarioContent scenarioContent)
    {
        string scenarioSourceCode = scenarioContent.Content;
        IReadOnlyCollection<ScenarioStepArguments> scenarioStepNodes = scenarioSourceCodeParser.Parse(scenarioSourceCode);
        IReadOnlyCollection<IValidationRule> steps = scenarioStepNodes.Select(scenarioStepParser.ParseScenarioStep).Cast<IValidationRule>().ToList();
        return steps;
    }
}