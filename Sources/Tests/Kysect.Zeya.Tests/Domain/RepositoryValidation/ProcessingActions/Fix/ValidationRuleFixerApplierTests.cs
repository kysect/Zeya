using FluentAssertions;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Fix;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Kysect.Zeya.Tests.Domain.RepositoryValidation.ProcessingActions.Fix;

public class ValidationRuleFixerApplierTests
{
    [Fact]
    public void Create_ForTestProject_ReturnCreatedFixer()
    {
        Assembly testAssembly = typeof(FakeValidationRuleFixer).Assembly;
        var fakeValidationRule = new FakeValidationRule();

        ServiceProvider provider = new ServiceCollection()
            .AddSingleton<FakeValidationRuleFixer>()
            .BuildServiceProvider();

        var validationRuleFixerApplier = ValidationRuleFixerApplier.Create(provider, testAssembly);

        validationRuleFixerApplier.IsFixerRegistered(fakeValidationRule).Should().BeTrue();
    }
}