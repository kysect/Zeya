using FluentAssertions;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Tools.Fakes;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tests.Domain.RepositoryValidation;

public class ValidationRuleParserTests
{
    private readonly ValidationRuleParser _validationRuleParser;

    public ValidationRuleParserTests()
    {
        _validationRuleParser = RepositoryValidationRuleProviderTestInstance.Create();
    }

    [Fact]
    public void GetValidationRules_ForDemoScenario_ReturnExpectedRuleCount()
    {
        var fileSystem = new FileSystem();
        string path = fileSystem.Path.Combine("Tools", "Assets", "ValidationScenario.yaml");

        string content = fileSystem.File.ReadAllText(path);
        var scenarioContent = new ScenarioContent(content);
        IReadOnlyCollection<IValidationRule> rules = _validationRuleParser.GetValidationRules(scenarioContent);

        rules.Should().HaveCount(14);
    }
}