namespace Kysect.Zeya.GithubIntegration.Abstraction;

public record struct GithubRepositoryName(string Owner, string Name)
{
    public readonly string FullName => $"{Owner}/{Name}";
}