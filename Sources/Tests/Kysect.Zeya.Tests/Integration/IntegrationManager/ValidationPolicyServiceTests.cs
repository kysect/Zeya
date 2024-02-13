using FluentAssertions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Integration.IntegrationManager;

public class ValidationPolicyServiceTests
{
    private readonly ValidationPolicyService _validationPolicyService;

    public ValidationPolicyServiceTests()
    {
        var dbContextFactory = ZeyaDbContextProvider.Create();
        _validationPolicyService = new ValidationPolicyService(dbContextFactory);
    }

    [Fact]
    public void Create_OnEmptyDatabase_EntityCreated()
    {
        ValidationPolicyEntity validationPolicyEntity = _validationPolicyService.Create("Policy", "Content");

        IReadOnlyCollection<ValidationPolicyEntity> policies = _validationPolicyService.Read();

        policies.Should().HaveCount(1);
    }
}