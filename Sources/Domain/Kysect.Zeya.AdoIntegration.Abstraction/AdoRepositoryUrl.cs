using Kysect.CommonLib.BaseTypes.Extensions;

namespace Kysect.Zeya.AdoIntegration.Abstraction;

public record struct AdoRepositoryUrl(string Collection, string Project, string Repository)
{
    public static AdoRepositoryUrl Parse(string serialized)
    {
        serialized.ThrowIfNull();

        string[] nameParts = serialized.Split("/", 3);
        if (nameParts.Length != 3)
            throw new ArgumentException($"Invalid ADO repository path: {serialized}");

        return new AdoRepositoryUrl(nameParts[0], nameParts[1], nameParts[2]);
    }

    public readonly string Serialize()
    {
        return $"{Collection}/{Project}/{Repository}";
    }

    public readonly string ToFullLink(string host)
    {
        host.ThrowIfNull();
        if (!host.EndsWith("/"))
            throw new ArgumentException($"HostUrl must end with '/'. Actual value: {host}");

        return $"{host}{Collection}/{Project}/_git/{Repository}";
    }
}