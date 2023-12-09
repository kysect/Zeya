using FluentAssertions;
using Kysect.Zeya.ValidationRules;

namespace Kysect.Zeya.Tests
{
    public class RepositoryValidationContextExtensionsTests
    {
        [Test]
        public void CreateScenarioContext_ReturnInitializedInstance()
        {
            var repositoryValidationContext = new RepositoryValidationContext(null, null);

            var scenarioContext = RepositoryValidationContextExtensions.CreateScenarioContext(repositoryValidationContext);
            
            scenarioContext.Should().NotBeNull();
        }
    }
}