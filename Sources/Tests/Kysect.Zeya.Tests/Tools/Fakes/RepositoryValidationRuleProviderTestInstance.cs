using Kysect.ScenarioLib.YamlParser;
using Kysect.ScenarioLib;
using Kysect.Zeya.RepositoryValidation;
using System.IO.Abstractions;
using Kysect.Zeya.RepositoryValidationRules.Rules;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public static class RepositoryValidationRuleProviderTestInstance
{
    public static RepositoryValidationRuleProvider Create()
    {
        var fileSystem = new FileSystem();
        var scenarioSourceProvider = new ScenarioContentProvider(fileSystem, fileSystem.Path.Combine("Tools", "Assets"));
        var yamlScenarioSourceCodeParser = new YamlScenarioContentParser();
        var scenarioStepReflectionParser = ScenarioContentStepReflectionDeserializer.Create(typeof(RuleDescription).Assembly);
        return new RepositoryValidationRuleProvider(scenarioSourceProvider, new ScenarioContentDeserializer(yamlScenarioSourceCodeParser, scenarioStepReflectionParser));
    }
}