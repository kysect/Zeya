namespace Kysect.Zeya.Abstractions.Models;

public record GithubRepository(string Owner, string Name)
{
    public string FullName => $"{Owner}/{Name}";
}