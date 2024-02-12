namespace Kysect.Zeya.LocalRepositoryAccess.Github;

public record struct GithubRepositoryName(string Owner, string Name)
{
    public readonly string FullName => $"{Owner}/{Name}";
}