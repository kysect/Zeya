namespace Kysect.Zeya.GithubIntegration.Abstraction.Models;

public record GithubRepository(string Owner, string Name)
{
    public string FullName => $"{Owner}/{Name}";
}