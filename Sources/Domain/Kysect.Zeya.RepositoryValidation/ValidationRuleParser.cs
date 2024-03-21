using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya.RepositoryValidation;

public class ValidationRuleParser(IScenarioContentDeserializer contentDeserializer)
{
    public IReadOnlyCollection<IValidationRule> GetValidationRules(string scenarioContent)
    {
        return contentDeserializer
            .Deserialize(scenarioContent)
            .Cast<IValidationRule>()
            .ToList();
    }
}