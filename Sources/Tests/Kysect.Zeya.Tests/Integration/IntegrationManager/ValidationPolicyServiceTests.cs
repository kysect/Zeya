using FluentAssertions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.IntegrationManager.Models;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Tools;
using Microsoft.EntityFrameworkCore;

namespace Kysect.Zeya.Tests.Integration.IntegrationManager;

public class ValidationPolicyServiceTests
{
    private readonly ValidationPolicyService _validationPolicyService;

    public ValidationPolicyServiceTests()
    {
        IDbContextFactory<ZeyaDbContext> dbContextFactory = ZeyaDbContextProvider.Create();
        _validationPolicyService = new ValidationPolicyService(dbContextFactory);
    }

    [Fact]
    public void Create_OnEmptyDatabase_EntityCreated()
    {
        ValidationPolicyEntity validationPolicyEntity = _validationPolicyService.CreatePolicy("Policy", "Content");

        IReadOnlyCollection<ValidationPolicyEntity> policies = _validationPolicyService.ReadPolicies();

        policies.Should().HaveCount(1);
    }

    [Fact]
    public void AddRepository_OnEmptyDatabase_RepositoryAdded()
    {
        ValidationPolicyEntity validationPolicyEntity = _validationPolicyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepository repository = _validationPolicyService.AddRepository(validationPolicyEntity.Id, "Owner", "Repository");

        IReadOnlyCollection<ValidationPolicyRepository> repositories = _validationPolicyService.GetRepositories(validationPolicyEntity.Id);

        repositories.Should().HaveCount(1);
    }

    [Fact]
    public void AddRepository_NoPolicy_ThrowException()
    {
        var argumentException = Assert.Throws<ArgumentException>(() =>
        {
            _validationPolicyService.AddRepository(Guid.Empty, "Owner", "Repository");
        });

        argumentException.Message.Should().Be("Policy not found");
    }

    [Fact]
    public void GetAllRulesForPolicy_DatabaseWithTwoRepositoryDiagnostic_ReturnDistinctRuleIds()
    {
        ValidationPolicyEntity validationPolicyEntity = _validationPolicyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepository repository = _validationPolicyService.AddRepository(validationPolicyEntity.Id, "Owner", "Repository");
        var firstDiagnostic = new RepositoryValidationDiagnostic("SRC0001", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var secondDiagnostic = new RepositoryValidationDiagnostic("SRC0002", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var report = new RepositoryValidationReport([firstDiagnostic, secondDiagnostic], []);

        _validationPolicyService.SaveReport(repository, report);

        var rules = _validationPolicyService.GetAllRulesForPolicy(repository.ValidationPolicyId);

        rules.Should().BeEquivalentTo("SRC0001", "SRC0002");
    }

    [Fact]
    public void SaveReport_OnEmptyDatabase_ReportSaved()
    {
        ValidationPolicyEntity validationPolicyEntity = _validationPolicyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepository repository = _validationPolicyService.AddRepository(validationPolicyEntity.Id, "Owner", "Repository");
        var validationDiagnostic = new RepositoryValidationDiagnostic("SRC0001", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var expected = new ValidationPolicyRepositoryDiagnostic(repository.Id, "SRC0001", "Warning");
        var report = new RepositoryValidationReport([validationDiagnostic], []);

        _validationPolicyService.SaveReport(repository, report);

        IReadOnlyCollection<ValidationPolicyRepositoryDiagnostic> diagnostics = _validationPolicyService.GetDiagnostics(repository.Id);

        diagnostics.Should().HaveCount(1);
        diagnostics.First().Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetDiagnosticsTable_ForTwoRepositoriesWithDiagnostics_ReturnTwoElementInListWithExpectedValue()
    {
        ValidationPolicyEntity validationPolicyEntity = _validationPolicyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepository firstRepository = _validationPolicyService.AddRepository(validationPolicyEntity.Id, "Owner", "Repository");
        ValidationPolicyRepository secondRepository = _validationPolicyService.AddRepository(validationPolicyEntity.Id, "Owner", "Repository2");
        var firstDiagnostic = new RepositoryValidationDiagnostic("SRC0001", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var secondDiagnostic = new RepositoryValidationDiagnostic("SRC0002", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var report = new RepositoryValidationReport([firstDiagnostic, secondDiagnostic], []);
        List<RepositoryDiagnosticTableRow> expected =
        [
            new RepositoryDiagnosticTableRow("Owner/Repository", new Dictionary<string, string>() { ["SRC0001"] = "Warning", ["SRC0002"] = "Warning" }),
            new RepositoryDiagnosticTableRow("Owner/Repository2", new Dictionary<string, string>() { ["SRC0001"] = "Warning", ["SRC0002"] = "Warning" })
        ];

        _validationPolicyService.SaveReport(firstRepository, report);
        _validationPolicyService.SaveReport(secondRepository, report);

        IReadOnlyCollection<RepositoryDiagnosticTableRow> diagnosticsTable = _validationPolicyService.GetDiagnosticsTable(validationPolicyEntity.Id);
        diagnosticsTable.Should().BeEquivalentTo(expected);
    }
}