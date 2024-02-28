using FluentAssertions;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Tools.Fakes;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tests.Domain.RepositoryValidation;

public class RepositoryValidationRuleProviderTests
{
    private readonly RepositoryValidationRuleProvider _repositoryValidationRuleProvider;

    public RepositoryValidationRuleProviderTests()
    {
        _repositoryValidationRuleProvider = RepositoryValidationRuleProviderTestInstance.Create();
    }

    [Fact]
    public void GetValidationRules_ForDemoScenario_ReturnExpectedRuleCount()
    {
        var fileSystem = new FileSystem();
        string path = fileSystem.Path.Combine("Tools", "Assets", "ValidationScenario.yaml");

        string content = fileSystem.File.ReadAllText(path);
        var scenarioContent = new ScenarioContent(content);
        IReadOnlyCollection<IValidationRule> rules = _repositoryValidationRuleProvider.GetValidationRules(scenarioContent);

        rules.Should().HaveCount(14);
    }
}