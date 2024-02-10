namespace Kysect.Zeya.GithubIntegration.Abstraction.Models;

public record struct GithubRepositoryName(string Owner, string Name)
{
    public readonly string FullName => $"{Owner}/{Name}";
}