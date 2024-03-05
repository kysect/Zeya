using FluentAssertions;
using Kysect.Zeya.Application;
using Kysect.Zeya.Application.LocalHandling;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Application;

public class PolicyRepositoryServiceTests
{
    private readonly PolicyService _policyService;
    private readonly PolicyRepositoryService _policyRepositoryService;

    public PolicyRepositoryServiceTests()
    {
        ZeyaDbContext context = ZeyaDbContextTestProvider.CreateContext();
        var validationPolicyService = new ValidationPolicyService(context);
        _policyService = new PolicyService(validationPolicyService, context);
        _policyRepositoryService = new PolicyRepositoryService(new ValidationPolicyRepositoryFactory(), context);
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

    [Fact]
    public async Task AddRepository_NoPolicy_ThrowException()
    {
        var argumentException = await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            return _policyRepositoryService.AddRepository(Guid.Empty, "Owner", "Repository");
        });

        argumentException.Message.Should().Be("Policy not found");
    }
}