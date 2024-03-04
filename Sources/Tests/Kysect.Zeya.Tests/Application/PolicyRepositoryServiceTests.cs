using FluentAssertions;
using Kysect.Zeya.Application;
using Kysect.Zeya.Application.LocalHandling;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Application;

public class PolicyRepositoryServiceTests
{
    private readonly PolicyService _policyService;
    private readonly PolicyRepositoryService _policyRepositoryService;

    public PolicyRepositoryServiceTests()
    {
        var validationPolicyService = new ValidationPolicyService(ZeyaDbContextTestProvider.CreateContext());
        _policyService = new PolicyService(validationPolicyService);
        _policyRepositoryService = new PolicyRepositoryService(validationPolicyService);
    }

    [Fact]
    public async Task AddRepository_WithValidData_ReturnRepositoryDto()
    {
        var githubOwner = "owner";
        var githubRepository = "repo";

        ValidationPolicyDto validationPolicyDto = await _policyService.CreatePolicy("Policy", "Content");
        var result = await _policyRepositoryService.AddRepository(validationPolicyDto.Id, githubOwner, githubRepository);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetRepositories_WithValidData_ReturnRepositoriesDto()
    {
        var githubOwner = "owner";
        var githubRepository = "repo";
        ValidationPolicyDto validationPolicyDto = await _policyService.CreatePolicy("Policy", "Content");
        await _policyRepositoryService.AddRepository(validationPolicyDto.Id, githubOwner, githubRepository);

        var result = await _policyRepositoryService.GetRepositories(validationPolicyDto.Id);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }
}