# Database architecture

```plantuml
entity ValidationPolicy {
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Content { get; init; }
}

entity ValidationPolicyRepository {
    public Guid Id { get; init; }
    public Guid ValidationPolicyId { get; init; }
    public string Repository { get; init; }
}

entity ValidationPolicyRepositoryDiagnostic {
    public Guid ValidationPolicyRepositoryId { get; init;}
    public string RuleId { get; init; }
    public Severity Severity { get; init; }

}

ValidationPolicy --|{ ValidationPolicyRepository
ValidationPolicyRepository --|{ ValidationPolicyRepositoryDiagnostic
```