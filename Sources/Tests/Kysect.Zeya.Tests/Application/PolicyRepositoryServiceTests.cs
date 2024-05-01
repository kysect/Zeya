using FluentAssertions;
using Kysect.Zeya.Application.DatabaseQueries;
using Kysect.Zeya.Application.LocalHandling;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Application;

public class PolicyRepositoryServiceTests
{
    private readonly PolicyService _policyService;
    private readonly PolicyRepositoryService _policyRepositoryService;
    private readonly ValidationPolicyDatabaseQueries _validationPolicyService;

    public PolicyRepositoryServiceTests()
    {
        ZeyaDbContext context = ZeyaDbContextTestProvider.CreateContext();
        _validationPolicyService = new ValidationPolicyDatabaseQueries(context);
        _policyService = new PolicyService(_validationPolicyService, context);
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

    [Fact]
    public async Task GetActions_TwoActionMessages_ReturnActionGrouped()
    {
        ValidationPolicyDto policy = await _policyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepositoryDto repository = await _policyRepositoryService.AddLocalRepository(policy.Id, "path", null);
        var firstResponse = new RepositoryProcessingResponse<bool>(
            "First",
            false,
            [new RepositoryProcessingMessage("QWE001", "Message", RepositoryValidationSeverity.Message), new RepositoryProcessingMessage("QWE002", "Message", RepositoryValidationSeverity.Message)]);

        var secondResponse = new RepositoryProcessingResponse<bool>(
            "Second",
            false,
            [new RepositoryProcessingMessage("QWE003", "Message", RepositoryValidationSeverity.Message)]);


        await _validationPolicyService.SaveProcessingActionResult(repository.Id, firstResponse);
        await _validationPolicyService.SaveProcessingActionResult(repository.Id, secondResponse);

        IReadOnlyCollection<ValidationPolicyRepositoryActionDto> result = await _policyRepositoryService.GetActions(policy.Id, repository.Id);

        result.Should().HaveCount(2);
        result.ElementAt(0).Title.Should().Be("First");
        result.ElementAt(0).Messages.Should().HaveCount(2);
        result.ElementAt(1).Title.Should().Be("Second");
        result.ElementAt(1).Messages.Should().HaveCount(1);
    }
}