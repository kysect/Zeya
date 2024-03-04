using FluentAssertions;
using Kysect.Zeya.Application;
using Kysect.Zeya.Application.LocalHandling;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Application;

public class PolicyServiceTests
{
    private readonly PolicyService _client;

    public PolicyServiceTests()
    {
        _client = new PolicyService(new ValidationPolicyService(ZeyaDbContextTestProvider.CreateContext()));
    }

    [Fact]
    public async Task CreatePolicy_ShouldReturnCreatedPolicy()
    {
        string name = "TestPolicy";
        string content = "TestContent";

        ValidationPolicyDto createdPolicy = await _client.CreatePolicy(name, content);

        createdPolicy.Should().NotBeNull();
        createdPolicy.Name.Should().Be(name);
        createdPolicy.Content.Should().Be(content);
    }

    [Fact]
    public async Task GetPolicy_ShouldReturnCorrectPolicy()
    {
        string name = "TestPolicy";
        string content = "TestContent";

        ValidationPolicyDto createdPolicy = await _client.CreatePolicy(name, content);
        ValidationPolicyDto retrievedPolicy = await _client.GetPolicy(createdPolicy.Id);

        retrievedPolicy.Should().NotBeNull();
        retrievedPolicy.Id.Should().Be(createdPolicy.Id);
        retrievedPolicy.Name.Should().Be(name);
        retrievedPolicy.Content.Should().Be(content);
    }

    [Fact]
    public async Task GetPolicies_ShouldReturnAllPolicies()
    {
        string name1 = "TestPolicy1";
        string content1 = "TestContent1";
        string name2 = "TestPolicy2";
        string content2 = "TestContent2";

        await _client.CreatePolicy(name1, content1);
        await _client.CreatePolicy(name2, content2);

        var policies = await _client.GetPolicies();

        policies.Should().NotBeNull();
        policies.Count.Should().Be(2);
        policies.Should().Contain(p => p.Name == name1 && p.Content == content1);
        policies.Should().Contain(p => p.Name == name2 && p.Content == content2);
    }
}