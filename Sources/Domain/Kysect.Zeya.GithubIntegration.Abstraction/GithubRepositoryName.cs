using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.Common;

namespace Kysect.Zeya.GithubIntegration.Abstraction;

public record struct GithubRepositoryName(string Owner, string Name)
{
    public static GithubRepositoryName Parse(string value)
    {
        value.ThrowIfNull();

        if (!value.Contains('/'))
            throw new ZeyaException("Repository does not contains '/'");

        string[] parts = value.Split('/', 2);

        return new GithubRepositoryName(parts[0], parts[1]);
    }

    public readonly string FullName => $"{Owner}/{Name}";
}