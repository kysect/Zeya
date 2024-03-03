using FluentAssertions;
using Kysect.Zeya.Client.Abstractions.Models;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Application.IntegrationManager;

public class ValidationPolicyServiceTests
{
    private readonly ValidationPolicyService _validationPolicyService;

    public ValidationPolicyServiceTests()
    {
        _validationPolicyService = new ValidationPolicyService(ZeyaDbContextProvider.CreateContext());
    }

    [Fact]
    public async Task Create_OnEmptyDatabase_EntityCreated()
    {
        ValidationPolicyEntity validationPolicyEntity = await _validationPolicyService.CreatePolicy("Policy", "Content");

        IReadOnlyCollection<ValidationPolicyEntity> policies = await _validationPolicyService.ReadPolicies();

        policies.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddRepository_OnEmptyDatabase_RepositoryAdded()
    {
        ValidationPolicyEntity validationPolicyEntity = await _validationPolicyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepository repository = await _validationPolicyService.AddRepository(validationPolicyEntity.Id, "Owner", "Repository");

        IReadOnlyCollection<ValidationPolicyRepository> repositories = await _validationPolicyService.GetRepositories(validationPolicyEntity.Id);

        repositories.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddRepository_NoPolicy_ThrowException()
    {
        var argumentException = await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            return _validationPolicyService.AddRepository(Guid.Empty, "Owner", "Repository");
        });

        argumentException.Message.Should().Be("Policy not found");
    }

    [Fact]
    public async Task GetAllRulesForPolicy_DatabaseWithTwoRepositoryDiagnostic_ReturnDistinctRuleIds()
    {
        ValidationPolicyEntity validationPolicyEntity = await _validationPolicyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepository repository = await _validationPolicyService.AddRepository(validationPolicyEntity.Id, "Owner", "Repository");
        var firstDiagnostic = new RepositoryValidationDiagnostic("SRC0001", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var secondDiagnostic = new RepositoryValidationDiagnostic("SRC0002", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var report = new RepositoryValidationReport(new[] { firstDiagnostic, secondDiagnostic }, Array.Empty<RepositoryValidationDiagnostic>());

        await _validationPolicyService.SaveReport(repository, report);

        var rules = await _validationPolicyService.GetAllRulesForPolicy(repository.ValidationPolicyId);

        rules.Should().BeEquivalentTo("SRC0001", "SRC0002");
    }

    [Fact]
    public async Task SaveReport_OnEmptyDatabase_ReportSaved()
    {
        ValidationPolicyEntity validationPolicyEntity = await _validationPolicyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepository repository = await _validationPolicyService.AddRepository(validationPolicyEntity.Id, "Owner", "Repository");
        var validationDiagnostic = new RepositoryValidationDiagnostic("SRC0001", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var expected = new ValidationPolicyRepositoryDiagnostic(repository.Id, "SRC0001", ValidationPolicyRepositoryDiagnosticSeverity.Warning);
        var report = new RepositoryValidationReport(new[] { validationDiagnostic }, Array.Empty<RepositoryValidationDiagnostic>());

        await _validationPolicyService.SaveReport(repository, report);

        IReadOnlyCollection<ValidationPolicyRepositoryDiagnostic> diagnostics = await _validationPolicyService.GetDiagnostics(repository.Id);

        diagnostics.Should().HaveCount(1);
        diagnostics.First().Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetDiagnosticsTable_ForTwoRepositoriesWithDiagnostics_ReturnTwoElementInListWithExpectedValue()
    {
        ValidationPolicyEntity validationPolicyEntity = await _validationPolicyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepository firstRepository = await _validationPolicyService.AddRepository(validationPolicyEntity.Id, "Owner", "Repository");
        ValidationPolicyRepository secondRepository = await _validationPolicyService.AddRepository(validationPolicyEntity.Id, "Owner", "Repository2");
        var firstDiagnostic = new RepositoryValidationDiagnostic("SRC0001", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var secondDiagnostic = new RepositoryValidationDiagnostic("SRC0002", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var report = new RepositoryValidationReport(new[] { firstDiagnostic, secondDiagnostic }, Array.Empty<RepositoryValidationDiagnostic>());
        var expected = new List<RepositoryDiagnosticTableRow>
            {
                new RepositoryDiagnosticTableRow("Owner", "Repository", new Dictionary<string, string> { ["SRC0001"] = "Warning", ["SRC0002"] = "Warning" }),
                new RepositoryDiagnosticTableRow("Owner", "Repository2", new Dictionary<string, string> { ["SRC0001"] = "Warning", ["SRC0002"] = "Warning" })
            };

        await _validationPolicyService.SaveReport(firstRepository, report);
        await _validationPolicyService.SaveReport(secondRepository, report);

        IReadOnlyCollection<RepositoryDiagnosticTableRow> diagnosticsTable = await _validationPolicyService.GetDiagnosticsTable(validationPolicyEntity.Id);
        diagnosticsTable.Should().BeEquivalentTo(expected);
    }
}
