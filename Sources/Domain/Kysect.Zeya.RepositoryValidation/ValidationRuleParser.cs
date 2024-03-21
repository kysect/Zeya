using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya.RepositoryValidation;

public class ValidationRuleParser(IScenarioContentDeserializer contentDeserializer)
{
    public IReadOnlyCollection<IValidationRule> GetValidationRules(ScenarioContent scenarioContent)
    {
        return contentDeserializer
            .Deserialize(scenarioContent.Content)
            .Cast<IValidationRule>()
            .ToList();
    }
}