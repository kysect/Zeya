using FluentAssertions;
using Kysect.Zeya.Application;
using Kysect.Zeya.Application.LocalHandling;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Application;

public class ValidationPolicyRepositoryApiLocalClientTests
{
    private readonly ValidationPolicyLocalClient _validationPolicyLocalClient;
    private readonly ValidationPolicyRepositoryApiLocalClient _validationPolicyRepositoryApiLocalClient;

    public ValidationPolicyRepositoryApiLocalClientTests()
    {
        var validationPolicyService = new ValidationPolicyService(ZeyaDbContextTestProvider.CreateContext());
        _validationPolicyLocalClient = new ValidationPolicyLocalClient(validationPolicyService);
        _validationPolicyRepositoryApiLocalClient = new ValidationPolicyRepositoryApiLocalClient(validationPolicyService);
    }

    [Fact]
    public async Task AddRepository_WithValidData_ReturnRepositoryDto()
    {
        var githubOwner = "owner";
        var githubRepository = "repo";

        ValidationPolicyDto validationPolicyDto = await _validationPolicyLocalClient.CreatePolicy("Policy", "Content");
        var result = await _validationPolicyRepositoryApiLocalClient.AddRepository(validationPolicyDto.Id, githubOwner, githubRepository);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetRepositories_WithValidData_ReturnRepositoriesDto()
    {
        var githubOwner = "owner";
        var githubRepository = "repo";
        ValidationPolicyDto validationPolicyDto = await _validationPolicyLocalClient.CreatePolicy("Policy", "Content");
        await _validationPolicyRepositoryApiLocalClient.AddRepository(validationPolicyDto.Id, githubOwner, githubRepository);

        var result = await _validationPolicyRepositoryApiLocalClient.GetRepositories(validationPolicyDto.Id);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }
}