using FluentAssertions;
using Kysect.Zeya.Application;
using Kysect.Zeya.Application.LocalHandling;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.DataAccess.Abstractions;
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
        var result = await _policyRepositoryService.AddGithubRepository(validationPolicyDto.Id, githubOwner, githubRepository, null);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetRepositories_WithValidData_ReturnRepositoriesDto()
    {
        var githubOwner = "owner";
        var githubRepository = "repo";
        ValidationPolicyDto validationPolicyDto = await _policyService.CreatePolicy("Policy", "Content");
        await _policyRepositoryService.AddGithubRepository(validationPolicyDto.Id, githubOwner, githubRepository, null);

        var result = await _policyRepositoryService.GetRepositories(validationPolicyDto.Id);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddRepository_NoPolicy_ThrowException()
    {
        var argumentException = await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            return _policyRepositoryService.AddGithubRepository(Guid.Empty, "Owner", "Repository", null);
        });

        argumentException.Message.Should().Be($"Cannot find {typeof(ValidationPolicyEntity).FullName} by key 00000000-0000-0000-0000-000000000000");
    }
}