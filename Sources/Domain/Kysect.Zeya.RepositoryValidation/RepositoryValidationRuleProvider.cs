using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya.RepositoryValidation;

public class RepositoryValidationRuleProvider(
    IScenarioContentProvider scenarioProvider,
    IScenarioContentDeserializer contentDeserializer)
{
    public ScenarioContent ReadScenario(string validationScenarioName)
    {
        string scenarioSourceCode = scenarioProvider.GetScenarioSourceCode(validationScenarioName);
        return new ScenarioContent(scenarioSourceCode);
    }

    public IReadOnlyCollection<IValidationRule> GetValidationRules(ScenarioContent scenarioContent)
    {
        return contentDeserializer
            .Deserialize(scenarioContent.Content)
            .Cast<IValidationRule>()
            .ToList();
    }
}